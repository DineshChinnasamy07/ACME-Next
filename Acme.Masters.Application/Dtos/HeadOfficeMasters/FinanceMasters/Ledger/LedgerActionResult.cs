/*  Class Name      : LedgerActionResult.cs
 *  Purpose         : Success text for Ledger actions
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Ledger
{
    public class LedgerActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int LedgerId { get; set; }
    }
}
