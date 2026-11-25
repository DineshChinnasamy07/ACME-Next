/*  Class Name      : CongregationLedgerMap.cs
 *  Purpose         : HO tenant CONGREGATION_LEDGER_MAP row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class CongregationLedgerMap
    {
        public int ConLedgerId { get; set; }

        public int LedgerId { get; set; }

        public int ProjectCatogoryGroupId { get; set; }
    }
}
