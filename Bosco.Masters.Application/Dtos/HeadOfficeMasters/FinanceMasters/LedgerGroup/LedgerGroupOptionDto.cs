/*  Class Name      : LedgerGroupOptionDto.cs
 *  Purpose         : Parent Ledger Group dropdown option
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class LedgerGroupOptionDto
    {
        public int GroupId { get; set; }

        public string LedgerGroup { get; set; } = string.Empty;
    }
}
