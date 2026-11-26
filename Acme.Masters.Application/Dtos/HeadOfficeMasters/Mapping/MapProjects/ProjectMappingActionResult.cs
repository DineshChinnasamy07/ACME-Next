/*  Class Name      : ProjectMappingActionResult.cs
 *  Purpose         : Result of a Project Mapping save
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectMappingActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        public int RowIndex { get; set; }
    }
}
