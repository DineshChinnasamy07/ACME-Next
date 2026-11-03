/*  Class Name      : RightsUser.cs
 *  Purpose         : rights_user row (role module/activity allow)
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Domain.Entities
{
    public class RightsUser
    {
        public int RightsId { get; set; }

        public string ModuleCode { get; set; } = string.Empty;

        public string ActivityCode { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public int Allow { get; set; }
    }
}
