import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api) — Head Office Finance Masters: FC Purpose and Generalate Ledger Group.
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

// api/fcpurposes — rows: { contributionId, code, fcPurpose }.
export const fcPurposeApi = {
    async fetchPurposes(session) {
        return asList(await masters(session).get('/api/fcpurposes'));
    },
    fetchPurpose(session, contributionId) {
        return masters(session).get(`/api/fcpurposes/${contributionId}`);
    },
    // request: { contributionId (0 = new), code, fcPurpose } → { success, message, contributionId }
    savePurpose(session, request) {
        return masters(session).post('/api/fcpurposes', request);
    },
    deletePurpose(session, contributionId) {
        return masters(session).delete(`/api/fcpurposes/${contributionId}`);
    },
};

// api/generalateledgergroups — rows: { conLedgerId, conLedgerCode, conLedgerName, conParentLedgerId,
// parentName, conMainParentId, hasChildren }. A top-level ledger has conParentLedgerId === conLedgerId.
export const generalateLedgerApi = {
    async fetchLedgers(session) {
        return asList(await masters(session).get('/api/generalateledgergroups'));
    },
    // Options: { id, name ("CODE  Name"), mainParentId }
    async fetchParentLedgers(session) {
        return asList(await masters(session).get('/api/generalateledgergroups/parents'));
    },
    fetchLedger(session, conLedgerId) {
        return masters(session).get(`/api/generalateledgergroups/${conLedgerId}`);
    },
    // request: { conLedgerId (0 = new), conLedgerCode, conLedgerName, conParentLedgerId } → { success, message, conLedgerId }
    saveLedger(session, request) {
        return masters(session).post('/api/generalateledgergroups', request);
    },
    deleteLedger(session, conLedgerId) {
        return masters(session).delete(`/api/generalateledgergroups/${conLedgerId}`);
    },
};
