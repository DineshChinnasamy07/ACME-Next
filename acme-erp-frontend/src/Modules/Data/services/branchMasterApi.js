import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api): generic Head Office -> Branch master mapping (api/mapbranchmasters).
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

// The master types the mapping engine supports (must match the backend whitelist).
export const BRANCH_MASTER_TYPES = [
    { key: 'ledger', label: 'Ledger' },
    { key: 'ledgergroup', label: 'Ledger Group' },
    { key: 'projectcategory', label: 'Project Category' },
    { key: 'legalentity', label: 'Legal Entity' },
    { key: 'fcpurpose', label: 'Purpose' },
];

export const branchMasterApi = {
    // Head office mapping screen.
    async fetchBranches(session) {
        return asList(await masters(session).get('/api/mapbranchmasters/branches'));
    },
    // All records of a master type: [{ id, code, name }]
    async fetchRecords(session, masterType) {
        return asList(await masters(session).get(`/api/mapbranchmasters/${masterType}/records`));
    },
    // Ids already mapped to a branch: [id, ...]
    async fetchMappedIds(session, masterType, branchId) {
        return asList(await masters(session).get(`/api/mapbranchmasters/${masterType}/mapped?branchId=${branchId}`));
    },
    // Replace the mapping for one branch + master type. request: { branchId, masterType, masterIds }
    saveMapping(session, request) {
        return masters(session).post('/api/mapbranchmasters', request);
    },

    // Branch login read: the records of a master type mapped to the logged-in branch.
    async fetchBranchMasters(session, masterType) {
        return asList(await masters(session).get(`/api/mapbranchmasters/branch/${masterType}`));
    },
};
