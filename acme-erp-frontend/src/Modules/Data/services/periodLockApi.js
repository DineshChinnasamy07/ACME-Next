import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api) — Head Office Finance Masters: Accounting Period and Lock Vouchers.
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

// Dates come back as ISO strings ("2025-04-01T00:00:00Z" or without the Z). Take the calendar part
// as-is (no Date() conversion, so the day never shifts with the browser time zone).
export function isoDay(value) {
    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(String(value || ''));
    if (!match) {
        return '';
    }
    const day = `${match[1]}-${match[2]}-${match[3]}`;
    return day === '0001-01-01' ? '' : day;
}

// yyyy-mm-dd (or ISO) → dd/mm/yyyy, the legacy grid format.
export function gridDate(value) {
    const day = isoDay(value);
    if (!day) {
        return '';
    }
    const [year, month, date] = day.split('-');
    return `${date}/${month}/${year}`;
}

// yyyy-mm-dd → value for a DateTime request field. Sent as UTC midnight: the backend maps DateTime to
// timestamptz, and Npgsql rejects DateTime values whose Kind is not Utc.
export function requestDate(day) {
    return day ? `${day}T00:00:00Z` : null;
}

// api/accountingperiods — rows: { accYearId, yearFrom, yearTo, status (1 active / 0 inactive), yearStatus }.
export const accountingPeriodApi = {
    async fetchPeriods(session) {
        return asList(await masters(session).get('/api/accountingperiods'));
    },
    // → { accYearId, yearFrom, yearTo, booksBeginningFrom, status, isFirstAccountingYear }
    fetchPeriod(session, accYearId) {
        return masters(session).get(`/api/accountingperiods/${accYearId}`);
    },
    // request: { accPeriodId (0 = new), yearFrom, yearTo } → { success, message, accYearId }
    savePeriod(session, request) {
        return masters(session).post('/api/accountingperiods', request);
    },
    deletePeriod(session, accYearId) {
        return masters(session).delete(`/api/accountingperiods/${accYearId}`);
    },
    // Makes this the active period (status 1) and deactivates all others. Only status 1 is accepted.
    activatePeriod(session, accYearId) {
        return masters(session).put(`/api/accountingperiods/${accYearId}/status`, { status: 1 });
    },
};

// api/lockvouchers — rows: { lockTransId, branchId, branchOffice, projectId, project, dateFrom, dateTo, reason, locked }.
export const lockVoucherApi = {
    async fetchLocks(session) {
        return asList(await masters(session).get('/api/lockvouchers'));
    },
    // Options: { id, name }
    async fetchBranches(session) {
        return asList(await masters(session).get('/api/lockvouchers/branches'));
    },
    // Projects mapped to the branch. For a new lock (lockTransId 0) projects already locked are left out.
    async fetchProjects(session, branchId, lockTransId = 0) {
        const query = `branchId=${encodeURIComponent(branchId)}&lockTransId=${encodeURIComponent(lockTransId || 0)}`;
        return asList(await masters(session).get(`/api/lockvouchers/projects?${query}`));
    },
    // → { lockTransId, branchId, projectId, dateFrom, dateTo, reason, lockType, lockByPortal } (no password / hint)
    fetchLock(session, lockTransId) {
        return masters(session).get(`/api/lockvouchers/${lockTransId}`);
    },
    // request: { lockTransId (0 = new), branchId, projectId, dateFrom, dateTo, password, passwordHint, reason }
    // → { success, message, lockTransId }. On edit a blank password / hint keeps the stored one.
    saveLock(session, request) {
        return masters(session).post('/api/lockvouchers', request);
    },
    deleteLock(session, lockTransId) {
        return masters(session).delete(`/api/lockvouchers/${lockTransId}`);
    },
};
