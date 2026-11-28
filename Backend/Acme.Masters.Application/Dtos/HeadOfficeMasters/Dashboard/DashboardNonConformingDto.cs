/*  Class Name      : DashboardNonConformingDto.cs
 *  Purpose         : Head Office dashboard - branch with months that have no vouchers (legacy Non Conforming Branches)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardNonConformingDto
    {
        public int BranchId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Incharge { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public List<string> PendingMonths { get; set; } = new List<string>();
    }
}
