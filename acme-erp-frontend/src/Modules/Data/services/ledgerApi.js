import { apiConfig } from '../../../Api_Config/apiConfig';
import { asList, serviceClient } from '../../Common/services/apiRequest';

// Masters service (Acme.Masters.Api) — Head Office Finance Masters: Ledger and Ledger Group.
function masters(session) {
    return serviceClient(apiConfig.mastersApiBaseUrl, session);
}

// api/ledgers — HO ledgers (STATUS = 0, non-branch).
// rows: { ledgerId, groupId, ledgerCode, ledgerName, group, ledgerType (GN/IK), ledgerSubType (GN/BK/FD),
//         bankAccountId, natureId, accessFlag, budgetGroupId, budgetSubGroupId }
export const ledgerApi = {
    async fetchLedgers(session) {
        return asList(await masters(session).get('/api/ledgers'));
    },
    // Ledger group dropdown (excludes the nature roots 1-4 and Bank Accounts 12): { groupId, natureId, ledgerGroup }
    async fetchGroupLookup(session) {
        return asList(await masters(session).get('/api/ledgers/groups'));
    },
    // MasterLedger entity: { ledgerId, ledgerCode, ledgerName, groupId, ledgerType, ledgerSubType, fdInvestmentTypeId,
    // bankAccountId, isCostCenter, isTdsLedger, notes, isBankInterestLedger, isBankFdPenaltyLedger, isBankSbInterestLedger,
    // isBankCommissionLedger, sortId, status, accessFlag, dateClosed, closedBy, isBranchLedger, isInkindLedger,
    // isDepreciationLedger, isAssetGainLedger, isAssetLossLedger, isDisposalLedger, budgetGroupId, budgetSubGroupId }
    fetchLedger(session, ledgerId) {
        return masters(session).get(`/api/ledgers/${ledgerId}`);
    },
    // request (LedgerSaveRequest): { ledgerId (0 = new), ledgerCode, ledgerName, groupId, ledgerType, bankAccountId,
    // isCostCentre, isTDSApplicable, ledgerNotes, isFDInterestLedger, isFDPenaltyLedger, isSBInterestLedger,
    // isBankCommissionLedger, isInKindLedger, isDepriciationLedger, isAssetGainLedger, isAssetLossLedger,
    // isAssetDisposalLedger, budgetGroupId, budgetSubGroupId, fdInvTypeId, ledgerClosedOn }
    // → { success, message, ledgerId }
    saveLedger(session, request) {
        return masters(session).post('/api/ledgers', request);
    },
    deleteLedger(session, ledgerId) {
        return masters(session).delete(`/api/ledgers/${ledgerId}`);
    },
};

// api/ledgergroups — flat tree source.
// rows: { groupId, groupCode, ledgerGroup, parentGroupId, parentGroup, natureId, mainGroupId, imageId, accessFlag, sortOrder }
// A nature root (Incomes / Expenses / Assets / Liabilities) has parentGroupId === groupId.
export const ledgerGroupApi = {
    async fetchGroups(session) {
        return asList(await masters(session).get('/api/ledgergroups'));
    },
    // Parent group dropdown (excludes Bank 12, Cash 13, FD 14): { groupId, ledgerGroup }
    async fetchParentLookup(session) {
        return asList(await masters(session).get('/api/ledgergroups/lookup'));
    },
    // Ledgers under the given groups (and their sub groups): { ledgerCode, ledgerName }
    async fetchLedgersUnder(session, groupIds) {
        const ids = Array.isArray(groupIds) ? groupIds.join(',') : String(groupIds);
        return asList(await masters(session).get(`/api/ledgergroups/ledgers?groupIds=${encodeURIComponent(ids)}`));
    },
    // MasterLedgerGroup entity: { groupId, groupCode, ledgerGroup, parentGroupId, natureId, mainGroupId, imageId, accessFlag, sortOrder }
    fetchGroup(session, groupId) {
        return masters(session).get(`/api/ledgergroups/${groupId}`);
    },
    // request: { groupId (0 = new), abbrevation, group, parentGroupId } → { success, message, groupId }
    saveGroup(session, request) {
        return masters(session).post('/api/ledgergroups', request);
    },
    deleteGroup(session, groupId) {
        return masters(session).delete(`/api/ledgergroups/${groupId}`);
    },
};

// MASTER_NATURE ids.
export const natureNames = { 1: 'Incomes', 2: 'Expenses', 3: 'Assets', 4: 'Liabilities' };

// ACCESS_FLAG: 0 user row, 1 editable system row, 2 read-only system row.
export const accessFlag = { accessable: 0, editable: 1, readonly: 2 };
