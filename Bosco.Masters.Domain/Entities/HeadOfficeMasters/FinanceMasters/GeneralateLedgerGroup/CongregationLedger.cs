/*  Class Name      : CongregationLedger.cs
 *  Purpose         : HO tenant CONGREGATION_LEDGER (Generalate Ledger Group) row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class CongregationLedger
    {
        public int ConLedgerId { get; set; }

        public string ConLedgerCode { get; set; } = string.Empty;

        public string ConLedgerName { get; set; } = string.Empty;

        public int ConParentLedgerId { get; set; }

        public int ConMainParentId { get; set; }
    }
}
