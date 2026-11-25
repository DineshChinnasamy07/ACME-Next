/*  Class Name      : LedgerUnderGroupDto.cs
 *  Purpose         : Ledger row under a selected Ledger Group
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class LedgerUnderGroupDto
    {
        public string LedgerCode { get; set; } = string.Empty;

        public string LedgerName { get; set; } = string.Empty;
    }
}
