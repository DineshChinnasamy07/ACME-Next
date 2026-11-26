/*  Class Name      : ProjectMappingLookupsDto.cs
 *  Purpose         : All lookups of the Project Mapping screen in one call
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectMappingLookupsDto
    {
        public List<ProjectMappingProjectDto> Projects { get; set; } = new List<ProjectMappingProjectDto>();

        public List<ProjectMappingOptionDto> Branches { get; set; } = new List<ProjectMappingOptionDto>();

        public List<ProjectMappingOptionDto> Locations { get; set; } = new List<ProjectMappingOptionDto>();

        public List<ProjectMappingOptionDto> Societies { get; set; } = new List<ProjectMappingOptionDto>();

        public List<ProjectMappingOptionDto> Categories { get; set; } = new List<ProjectMappingOptionDto>();

        public List<ProjectMappingOptionDto> Divisions { get; set; } = new List<ProjectMappingOptionDto>();

        public List<ProjectMappingUserOptionDto> Users { get; set; } = new List<ProjectMappingUserOptionDto>();
    }
}
