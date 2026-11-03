/*  Class Name      : RightsModuleDto.cs
 *  Purpose         : Module group with activities for User Rights
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class RightsModuleDto
    {
        public string ModuleCode { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public IReadOnlyList<RightsActivityItemDto> Activities { get; set; } = Array.Empty<RightsActivityItemDto>();
    }
}
