/*  Class Name      : LedgerGroupActionResult.cs
 *  Purpose         : Success text for Ledger Group actions
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class LedgerGroupActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int GroupId { get; set; }
    }
}
