/*  Class Name      : ProjectMappingRowDto.cs
 *  Purpose         : One branch row of a project mapping
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectMappingRowDto
    {
        public int BranchId { get; set; }

        public int LocationId { get; set; }

        public int SocietyId { get; set; }

        public int CategoryId { get; set; }

        public int DivisionId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? UserId { get; set; }

        public bool HasTransactions { get; set; }
    }
}
