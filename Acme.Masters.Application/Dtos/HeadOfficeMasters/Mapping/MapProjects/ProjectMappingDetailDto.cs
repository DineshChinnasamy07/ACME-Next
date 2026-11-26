/*  Class Name      : ProjectMappingDetailDto.cs
 *  Purpose         : Saved branch rows of one project (IsSaved false = rows built from legacy PROJECT_BRANCH)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectMappingDetailDto
    {
        public int ProjectId { get; set; }

        public bool IsSaved { get; set; }

        public List<ProjectMappingRowDto> Rows { get; set; } = new List<ProjectMappingRowDto>();
    }
}
