import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api) — Head Office Statutory Masters: TDS Policy (TDS_POLICY + TDS_TAX_RATE).
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

// api/tdspolicies
// List rows: { tdsPolicyId, deducteeTypeId, deducteeType, naturePayId, natureOfPayment, paymentCode, tdsSectionId,
//   sectionCode, sectionName, applicableFrom, taxRates: [{ taxRateId, tdsTaxTypeId, taxTypeName, tdsRate, tdsExemptionLimit }] }
export const tdsPolicyApi = {
    async fetchPolicies(session) {
        return asList(await masters(session).get('/api/tdspolicies'));
    },
    // → { deducteeTypes, natureOfPayments, taxTypes } — each [{ id, name, detail }], active rows only.
    // detail: deductee "Resident / Company", nature of payment = section code.
    async fetchLookups(session) {
        const result = await masters(session).get('/api/tdspolicies/lookups');
        return {
            deducteeTypes: asList(result && result.deducteeTypes),
            natureOfPayments: asList(result && result.natureOfPayments),
            taxTypes: asList(result && result.taxTypes),
        };
    },
    // → { tdsPolicyId, deducteeTypeId, naturePayId, applicableFrom, taxRates: [...] }
    fetchPolicy(session, tdsPolicyId) {
        return masters(session).get(`/api/tdspolicies/${tdsPolicyId}`);
    },
    // request: { tdsPolicyId (0 = new), deducteeTypeId, naturePayId, applicableFrom,
    //   taxRates: [{ tdsTaxTypeId, tdsRate, tdsExemptionLimit }] } → { success, message, tdsPolicyId }.
    // Policy + rates are saved in one transaction; on edit the rate rows are replaced.
    savePolicy(session, request) {
        return masters(session).post('/api/tdspolicies', request);
    },
    deletePolicy(session, tdsPolicyId) {
        return masters(session).delete(`/api/tdspolicies/${tdsPolicyId}`);
    },
};
