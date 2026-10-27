/*  Class Name      : Role.cs
 *  Purpose         : user_role row joined from ROLE_ID
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
namespace Bosco.Auth.Domain.Entities
{
    public class Role
    {
        public int UserRoleId { get; set; }

        public string UserRole { get; set; } = string.Empty;
    }
}
