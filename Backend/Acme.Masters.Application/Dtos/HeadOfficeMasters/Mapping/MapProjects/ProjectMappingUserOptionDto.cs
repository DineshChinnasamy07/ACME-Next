/*  Class Name      : ProjectMappingUserOptionDto.cs
 *  Purpose         : User in charge option of one branch for Project Mapping
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectMappingUserOptionDto
    {
        public int Id { get; set; }

        public int BranchId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
    }
}
