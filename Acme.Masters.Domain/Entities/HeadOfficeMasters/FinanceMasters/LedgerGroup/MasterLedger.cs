/*  Class Name      : MasterLedger.cs
 *  Purpose         : HO tenant MASTER_LEDGER row for Ledger CRUD and Ledger Group checks
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class MasterLedger
    {
        public int LedgerId { get; set; }

        public string? LedgerCode { get; set; }

        public string LedgerName { get; set; } = string.Empty;

        public int GroupId { get; set; }

        public string LedgerType { get; set; } = "GN";

        public string LedgerSubType { get; set; } = "GN";

        public int FdInvestmentTypeId { get; set; }

        public int BankAccountId { get; set; }

        public int IsCostCenter { get; set; }

        public int IsTdsLedger { get; set; }

        public string? Notes { get; set; }

        public int IsBankInterestLedger { get; set; }

        public int IsBankFdPenaltyLedger { get; set; }

        public int IsBankSbInterestLedger { get; set; }

        public int IsBankCommissionLedger { get; set; }

        public int SortId { get; set; }

        public int Status { get; set; }

        public int AccessFlag { get; set; }

        public DateTime? DateClosed { get; set; }

        public int ClosedBy { get; set; }

        public int IsBranchLedger { get; set; }

        public int IsInkindLedger { get; set; }

        public int IsDepreciationLedger { get; set; }

        public int IsAssetGainLedger { get; set; }

        public int IsAssetLossLedger { get; set; }

        public int IsDisposalLedger { get; set; }

        public int BudgetGroupId { get; set; }

        public int BudgetSubGroupId { get; set; }
    }
}
