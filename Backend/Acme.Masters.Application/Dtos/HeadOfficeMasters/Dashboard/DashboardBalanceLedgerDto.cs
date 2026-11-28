/*  Class Name      : DashboardBalanceLedgerDto.cs
 *  Purpose         : Head Office dashboard - closing balance of one cash / bank / FD ledger
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardBalanceLedgerDto
    {
        public int LedgerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Mode { get; set; } = "DR";
    }
}
