/*  Class Name      : DashboardSummaryRequest.cs
 *  Purpose         : Head Office dashboard - summary criteria (0 = all / active year)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardSummaryRequest
    {
        public int YearId { get; set; }

        public int BranchId { get; set; }

        public int ProjectId { get; set; }

        public DateTime? PendingFrom { get; set; }

        public DateTime? PendingTo { get; set; }
    }
}
