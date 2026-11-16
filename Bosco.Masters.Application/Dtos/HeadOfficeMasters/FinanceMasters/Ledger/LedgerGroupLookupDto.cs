/*  Class Name      : LedgerGroupLookupDto.cs
 *  Purpose         : Ledger Group option for Ledger add/edit dropdown
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Ledger
{
    public class LedgerGroupLookupDto
    {
        public int GroupId { get; set; }

        public int NatureId { get; set; }

        public string LedgerGroup { get; set; } = string.Empty;
    }
}
