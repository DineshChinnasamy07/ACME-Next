/*  Class Name      : UpdateUserRightsItem.cs
 *  Purpose         : One changed right for UpdateUserRights
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Auth.Application.Dtos
{
    public class UpdateUserRightsItem
    {
        public string ModuleCode { get; set; } = string.Empty;

        public string ActivityCode { get; set; } = string.Empty;

        public bool Allow { get; set; }
    }
}
