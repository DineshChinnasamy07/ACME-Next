/*  Class Name      : ProjectMappingSaveRequest.cs
 *  Purpose         : Save request: the full list of branch rows of one project (insert or update in one Save)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectMappingSaveRequest
    {
        public int ProjectId { get; set; }

        public List<ProjectMappingRowDto> Rows { get; set; } = new List<ProjectMappingRowDto>();
    }
}
