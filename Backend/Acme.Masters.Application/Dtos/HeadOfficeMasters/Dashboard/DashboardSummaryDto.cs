/*  Class Name      : DashboardSummaryDto.cs
 *  Purpose         : Head Office dashboard - everything the Home page shows for the chosen year / branch / project
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardSummaryDto
    {
        public int YearId { get; set; }

        public DateTime YearFrom { get; set; }

        public DateTime YearTo { get; set; }

        // Month keys "yyyy-MM" from YearFrom to YearTo; Counts / MonthTotals follow this order.
        public List<string> Months { get; set; } = new List<string>();

        // "branch" when all branches are shown, "project" when one branch is chosen.
        public string RowKind { get; set; } = "branch";

        public List<DashboardStatusRowDto> Rows { get; set; } = new List<DashboardStatusRowDto>();

        public List<int> MonthTotals { get; set; } = new List<int>();

        public int TotalVouchers { get; set; }

        public int Branches { get; set; }

        public int BranchesWithData { get; set; }

        public int Projects { get; set; }

        public DateTime? LastVoucherDate { get; set; }

        public DateTime BalanceAsOn { get; set; }

        public List<DashboardBalanceDto> Balances { get; set; } = new List<DashboardBalanceDto>();

        public DateTime PendingFrom { get; set; }

        public DateTime PendingTo { get; set; }

        public List<DashboardNonConformingDto> NonConforming { get; set; } = new List<DashboardNonConformingDto>();
    }
}
