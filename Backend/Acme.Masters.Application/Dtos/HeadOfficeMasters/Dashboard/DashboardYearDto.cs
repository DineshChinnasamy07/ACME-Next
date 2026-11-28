/*  Class Name      : DashboardYearDto.cs
 *  Purpose         : Head Office dashboard - accounting year option
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardYearDto
    {
        public int Id { get; set; }

        public DateTime YearFrom { get; set; }

        public DateTime YearTo { get; set; }

        public bool IsActive { get; set; }
    }
}
