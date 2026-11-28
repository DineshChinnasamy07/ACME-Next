/*  Class Name      : DashboardBalanceDto.cs
 *  Purpose         : Head Office dashboard - closing balance of one group (Cash, Bank, Fixed Deposit)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardBalanceDto
    {
        public int GroupId { get; set; }

        public string Group { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Mode { get; set; } = "DR";

        public List<DashboardBalanceLedgerDto> Ledgers { get; set; } = new List<DashboardBalanceLedgerDto>();
    }
}
