/*  Class Name      : DashboardStatusRowDto.cs
 *  Purpose         : Head Office dashboard - data status row (voucher count per month for one branch or project)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard
{
    public class DashboardStatusRowDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<int> Counts { get; set; } = new List<int>();

        public int Total { get; set; }
    }
}
