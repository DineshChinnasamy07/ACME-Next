/*  Class Name      : RoleSaveRequest.cs
 *  Purpose         : SaveUserRoles request body (add when UserRoleId is 0)
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class RoleSaveRequest
    {
        public int UserRoleId { get; set; }

        public string UserRoleName { get; set; } = string.Empty;
    }
}
