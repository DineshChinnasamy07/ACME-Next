/*  Class Name      : RightsRoleItemDto.cs
 *  Purpose         : Role row for the User Rights tree (GetUsersType)
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class RightsRoleItemDto
    {
        public int UserRoleId { get; set; }

        public string UserRole { get; set; } = string.Empty;
    }
}
