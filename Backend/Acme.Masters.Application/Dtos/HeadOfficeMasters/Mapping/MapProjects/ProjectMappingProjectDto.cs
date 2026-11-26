/*  Class Name      : ProjectMappingProjectDto.cs
 *  Purpose         : Project option for Project Mapping with its creation details and mapped branch count
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectMappingProjectDto
    {
        public int ProjectId { get; set; }

        public string ProjectCode { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public DateTime? CreatedOn { get; set; }

        public string Description { get; set; } = string.Empty;

        public int MappedBranches { get; set; }
    }
}
