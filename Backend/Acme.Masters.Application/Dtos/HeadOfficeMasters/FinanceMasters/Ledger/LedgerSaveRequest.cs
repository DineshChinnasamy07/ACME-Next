/*  Class Name      : LedgerSaveRequest.cs
 *  Purpose         : SaveLedgerDetails request body
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Ledger
{
    public class LedgerSaveRequest
    {
        public int LedgerId { get; set; }

        public string? LedgerCode { get; set; }

        public string? LedgerName { get; set; }

        public int GroupId { get; set; }

        public string? LedgerType { get; set; }

        public int BankAccountId { get; set; }

        public int IsCostCentre { get; set; }

        public int IsTDSApplicable { get; set; }

        public string? LedgerNotes { get; set; }

        public int IsFDInterestLedger { get; set; }

        public int IsFDPenaltyLedger { get; set; }

        public int IsSBInterestLedger { get; set; }

        public int IsBankCommissionLedger { get; set; }

        public int IsInKindLedger { get; set; }

        public int IsDepriciationLedger { get; set; }

        public int IsAssetGainLedger { get; set; }

        public int IsAssetLossLedger { get; set; }

        public int IsAssetDisposalLedger { get; set; }

        public int BudgetGroupId { get; set; }

        public int BudgetSubGroupId { get; set; }

        public int FDInvTypeId { get; set; }

        public DateTime? LedgerClosedOn { get; set; }
    }
}
