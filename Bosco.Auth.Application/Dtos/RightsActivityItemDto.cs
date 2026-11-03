/*  Class Name      : RightsActivityItemDto.cs
 *  Purpose         : One activity checkbox under a module
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class RightsActivityItemDto
    {
        public string ActivityCode { get; set; } = string.Empty;

        public string Activity { get; set; } = string.Empty;

        public bool Allow { get; set; }
    }
}
