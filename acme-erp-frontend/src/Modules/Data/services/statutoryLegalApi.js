import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api) — Head Office Statutory Masters: Legal Entity, Governing Bodies,
// Map Governing Bodies.
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

// api/legalentities — rows: { customerId, societyName, regNo, regDate, place, state, country }.
export const legalEntityApi = {
    async fetchLegalEntities(session) {
        return asList(await masters(session).get('/api/legalentities'));
    },
    // → { associationNatures: [{ id, name }], denominations: [{ id, name }] }
    fetchLookups(session) {
        return masters(session).get('/api/legalentities/lookups');
    },
    // → { customerId, societyName, contactPerson, address, place, state, country, pincode, phone, fax, email, url,
    //     regNo, regDate, permissionNo, permissionDate, a12No, panNo, girNo, tanNo, fcriNo, fcriRegDate,
    //     eightyGNo, eightyGNoRegDate, associationNature ("0,3"), otherAssociationNature, denomination,
    //     otherDenomination, isFoundation, principalActivity }
    fetchLegalEntity(session, customerId) {
        return masters(session).get(`/api/legalentities/${customerId}`);
    },
    // request: same shape as the detail (customerId 0 = new) → { success, message, customerId }
    saveLegalEntity(session, request) {
        return masters(session).post('/api/legalentities', request);
    },
    deleteLegalEntity(session, customerId) {
        return masters(session).delete(`/api/legalentities/${customerId}`);
    },
};

// api/governingbodies — rows: { executiveId, executive, name, nationality, occupation, role, countryId, stateId,
// dateOfAppointment, dateOfExit }.
export const governingBodyApi = {
    async fetchMembers(session) {
        return asList(await masters(session).get('/api/governingbodies'));
    },
    // → { executiveId, executive, name, dateOfBirth, religion, role, nationality, occupation, association,
    //     officeBearer, place, countryId, stateId, address, pinCode, panSsn, phone, fax, email, url,
    //     dateOfAppointment, dateOfExit, notes }
    fetchMember(session, executiveId) {
        return masters(session).get(`/api/governingbodies/${executiveId}`);
    },
    // request: same shape as the detail (executiveId 0 = new) → { success, message, executiveId }
    saveMember(session, request) {
        return masters(session).post('/api/governingbodies', request);
    },
    deleteMember(session, executiveId) {
        return masters(session).delete(`/api/governingbodies/${executiveId}`);
    },
};

// api/mapgoverningbodies
export const mapGoverningBodyApi = {
    // [{ id, name }]
    async fetchLegalEntities(session) {
        return asList(await masters(session).get('/api/mapgoverningbodies/legalentities'));
    },
    // [{ executiveId, executive, role, dateOfAppointment, dateOfExit, selected }]
    async fetchMembers(session, customerId) {
        return asList(await masters(session).get(`/api/mapgoverningbodies/${customerId}`));
    },
    // request: { customerId, executiveIds: [] } → { success, message, customerId, mappedCount }
    saveMapping(session, request) {
        return masters(session).post('/api/mapgoverningbodies', request);
    },
    unmapAll(session, customerId) {
        return masters(session).delete(`/api/mapgoverningbodies/${customerId}`);
    },
};

// API DateTime ("2019-04-01T00:00:00") → yyyy-mm-dd for <input type="date">.
export function toInputDate(value) {
    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(String(value || ''));
    if (!match) {
        return '';
    }
    const day = `${match[1]}-${match[2]}-${match[3]}`;
    return day === '0001-01-01' ? '' : day;
}

// API DateTime → dd/mm/yyyy (legacy grid format).
export function toGridDate(value) {
    const day = toInputDate(value);
    if (!day) {
        return '';
    }
    const [year, month, date] = day.split('-');
    return `${date}/${month}/${year}`;
}

// yyyy-mm-dd → DateTime request value. Sent as UTC midnight (the backend writes UTC-kind dates).
export function requestDate(day) {
    return day ? `${day}T00:00:00Z` : null;
}

// Local date as yyyy-mm-dd.
export function today() {
    const now = new Date();
    const pad = (value) => String(value).padStart(2, '0');
    return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}

export const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
