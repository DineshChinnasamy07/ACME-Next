/*  Class Name      : ProjectMappingOptionDto.cs
 *  Purpose         : Lookup option (id, code, name) for Project Mapping
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectMappingOptionDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        // Locations only: the branch the location was created for (0 = Primary, shared by every branch).
        public int BranchId { get; set; }
    }
}
