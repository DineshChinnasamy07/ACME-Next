/*  Class Name      : RoleActionResult.cs
 *  Purpose         : Success text for SaveUserRoles and DeleteUserRole
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class RoleActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int UserRoleId { get; set; }
    }
}
