/*  Class Name      : DashboardProjectOptionDto.cs
 *  Purpose         : Head Office dashboard - project option with the branches it is mapped to
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardProjectOptionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<int> BranchIds { get; set; } = new List<int>();
    }
}
