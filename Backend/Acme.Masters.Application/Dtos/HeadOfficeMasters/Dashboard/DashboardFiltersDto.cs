/*  Class Name      : DashboardFiltersDto.cs
 *  Purpose         : Head Office dashboard - filter lookups (years, branches, projects)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardFiltersDto
    {
        public List<DashboardYearDto> Years { get; set; } = new List<DashboardYearDto>();

        public List<DashboardOptionDto> Branches { get; set; } = new List<DashboardOptionDto>();

        public List<DashboardProjectOptionDto> Projects { get; set; } = new List<DashboardProjectOptionDto>();
    }
}
