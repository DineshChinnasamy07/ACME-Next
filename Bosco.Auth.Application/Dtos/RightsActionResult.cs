/*  Class Name      : RightsActionResult.cs
 *  Purpose         : Success text for UpdateUserRights
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class RightsActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
