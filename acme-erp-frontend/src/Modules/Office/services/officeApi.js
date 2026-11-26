import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Organization service (Acme.Organization.Api): head offices, branch offices, countries.
function org(session) {
    return serviceClient(apiConfig.organizationApiBaseUrl, session);
}

export const headOfficeApi = {
    async fetchHeadOffices(session) {
        return asList(await org(session).get('/api/headoffices'));
    },
    async fetchActiveHeadOffices(session) {
        return asList(await org(session).get('/api/headoffices/active'));
    },
    async fetchTypes(session) {
        return asList(await org(session).get('/api/headoffices/types'));
    },
    fetchHeadOffice(session, headOfficeId) {
        return org(session).get(`/api/headoffices/${headOfficeId}`);
    },
    saveHeadOffice(session, request) {
        return org(session).post('/api/headoffices', request);
    },
    updateStatus(session, headOfficeCode, status) {
        return org(session).put(`/api/headoffices/${encodeURIComponent(headOfficeCode)}/status`, { status });
    },
    deleteHeadOffice(session, headOfficeCode) {
        return org(session).delete(`/api/headoffices/${encodeURIComponent(headOfficeCode)}`);
    },
};

export const branchOfficeApi = {
    async fetchBranchOffices(session) {
        return asList(await org(session).get('/api/branchoffices'));
    },
    fetchBranchOffice(session, branchOfficeId) {
        return org(session).get(`/api/branchoffices/${branchOfficeId}`);
    },
    saveBranchOffice(session, request) {
        return org(session).post('/api/branchoffices', request);
    },
    updateStatus(session, branchOfficeCode, status) {
        return org(session).put(`/api/branchoffices/${encodeURIComponent(branchOfficeCode)}/status`, { status });
    },
    deleteBranchOffice(session, branchOfficeCode) {
        return org(session).delete(`/api/branchoffices/${encodeURIComponent(branchOfficeCode)}`);
    },
};

// Branch locations of the signed-in head office (api/branchlocations).
export const branchLocationApi = {
    async fetchLocations(session) {
        return asList(await org(session).get('/api/branchlocations'));
    },
    async fetchBranches(session) {
        return asList(await org(session).get('/api/branchlocations/branches'));
    },
    fetchLocation(session, locationId) {
        return org(session).get(`/api/branchlocations/${locationId}`);
    },
    saveLocation(session, request) {
        return org(session).post('/api/branchlocations', request);
    },
    deleteLocation(session, locationId) {
        return org(session).delete(`/api/branchlocations/${locationId}`);
    },
};

export const countryApi = {
    async fetchCountries(session) {
        return asList(await org(session).get('/api/countries'));
    },
    async fetchStates(session, countryId) {
        return asList(await org(session).get(`/api/countries/${countryId}/states`));
    },
};

// Office status codes used by the backend: 1 Created, 2 Activated, 3 DeActivated.
export const officeStatus = { created: 1, activated: 2, deactivated: 3 };
