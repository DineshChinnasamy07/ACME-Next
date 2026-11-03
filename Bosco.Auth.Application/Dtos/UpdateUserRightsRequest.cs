/*  Class Name      : UpdateUserRightsRequest.cs
 *  Purpose         : Batch save body for UpdateUserRights
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class UpdateUserRightsRequest
    {
        public int RoleId { get; set; }

        public IList<UpdateUserRightsItem> Rights { get; set; } = new List<UpdateUserRightsItem>();
    }
}
