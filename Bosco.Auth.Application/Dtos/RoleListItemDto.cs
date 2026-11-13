/*  Class Name      : RoleListItemDto.cs
 *  Purpose         : One row for the User Role list grid
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class RoleListItemDto
    {
        public int UserRoleId { get; set; }

        public string Name { get; set; } = string.Empty;

        // Admin / Branch Admin roles cannot be edited or deleted. Filled by the Head Office role list.
        public bool IsSystem { get; set; }

        // Number of users assigned to the role. Filled by the Head Office role list.
        public int UserCount { get; set; }
    }
}
