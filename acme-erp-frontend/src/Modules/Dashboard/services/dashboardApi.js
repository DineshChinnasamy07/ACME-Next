import { apiConfig } from '../../../Api_Config/apiConfig';
import { serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api): Head Office Home dashboard (api/hodashboard).
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

export const dashboardApi = {
    // { years: [{ id, yearFrom, yearTo, isActive }], branches: [{ id, code, name }], projects: [{ id, name, branchIds }] }
    fetchFilters(session) {
        return masters(session).get('/api/hodashboard/filters');
    },
    // criteria: { yearId, branchId, projectId, pendingFrom, pendingTo } (0 / '' = all, active year, default period)
    fetchSummary(session, criteria) {
        const query = new URLSearchParams();
        Object.entries(criteria || {}).forEach(([key, value]) => {
            if (value !== undefined && value !== null && value !== '' && value !== 0) {
                query.set(key, String(value));
            }
        });
        const text = query.toString();
        return masters(session).get(`/api/hodashboard/summary${text ? `?${text}` : ''}`);
    },
};
