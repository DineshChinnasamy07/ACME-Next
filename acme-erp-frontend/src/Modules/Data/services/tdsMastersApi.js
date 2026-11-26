import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api) — Head Office Statutory Masters: TDS Section, Nature of Payments,
// Deductee Type and Duty Tax. Status is 1 = Active, 0 = Inactive on every master.
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

// api/tdssections — rows: { tdsSectionId, code, sectionName, status, statusName }.
export const tdsSectionApi = {
    async fetchSections(session) {
        return asList(await masters(session).get('/api/tdssections'));
    },
    fetchSection(session, tdsSectionId) {
        return masters(session).get(`/api/tdssections/${tdsSectionId}`);
    },
    // request: { tdsSectionId (0 = new), code, sectionName, status } → { success, message, tdsSectionId }
    saveSection(session, request) {
        return masters(session).post('/api/tdssections', request);
    },
    deleteSection(session, tdsSectionId) {
        return masters(session).delete(`/api/tdssections/${tdsSectionId}`);
    },
};

// api/natureofpayments — rows: { naturePayId, paymentCode, name, tdsSectionId, sectionCode, sectionName,
// description, status, statusName }.
export const natureOfPaymentApi = {
    async fetchNatures(session) {
        return asList(await masters(session).get('/api/natureofpayments'));
    },
    // Active sections: { tdsSectionId, code, name ("CODE - SECTION_NAME") }
    async fetchSections(session) {
        return asList(await masters(session).get('/api/natureofpayments/sections'));
    },
    fetchNature(session, naturePayId) {
        return masters(session).get(`/api/natureofpayments/${naturePayId}`);
    },
    // request: { naturePayId (0 = new), name, paymentCode, tdsSectionId, description, status }
    //   → { success, message, naturePayId }
    saveNature(session, request) {
        return masters(session).post('/api/natureofpayments', request);
    },
    deleteNature(session, naturePayId) {
        return masters(session).delete(`/api/natureofpayments/${naturePayId}`);
    },
};

// api/deducteetypes — rows: { deducteeTypeId, name, residentialStatus (0 Resident / 1 Non Resident),
// residentialStatusName, deducteeType (0 Company / 1 Non Company), deducteeTypeName, status, statusName }.
export const deducteeTypeApi = {
    async fetchDeducteeTypes(session) {
        return asList(await masters(session).get('/api/deducteetypes'));
    },
    fetchDeducteeType(session, deducteeTypeId) {
        return masters(session).get(`/api/deducteetypes/${deducteeTypeId}`);
    },
    // request: { deducteeTypeId (0 = new), name, residentialStatus, deducteeType, status }
    //   → { success, message, deducteeTypeId }
    saveDeducteeType(session, request) {
        return masters(session).post('/api/deducteetypes', request);
    },
    deleteDeducteeType(session, deducteeTypeId) {
        return masters(session).delete(`/api/deducteetypes/${deducteeTypeId}`);
    },
};

// api/dutytaxes — rows: { tdsDutyTaxTypeId, taxTypeName, status, statusName, isSystem }.
// Built-in types (isSystem) can only change status; they cannot be renamed or deleted.
export const dutyTaxApi = {
    async fetchDutyTaxes(session) {
        return asList(await masters(session).get('/api/dutytaxes'));
    },
    fetchDutyTax(session, tdsDutyTaxTypeId) {
        return masters(session).get(`/api/dutytaxes/${tdsDutyTaxTypeId}`);
    },
    // request: { tdsDutyTaxTypeId (0 = new), taxTypeName, status } → { success, message, tdsDutyTaxTypeId }
    saveDutyTax(session, request) {
        return masters(session).post('/api/dutytaxes', request);
    },
    deleteDutyTax(session, tdsDutyTaxTypeId) {
        return masters(session).delete(`/api/dutytaxes/${tdsDutyTaxTypeId}`);
    },
};
