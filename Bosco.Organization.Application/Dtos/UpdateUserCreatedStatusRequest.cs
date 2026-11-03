/*  Class Name      : UpdateUserCreatedStatusRequest.cs
 *  Purpose         : UpdateUserCreatedStatus body (USER_CREATED_STATUS)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class UpdateUserCreatedStatusRequest
    {
        /// <summary>
        /// 0 = not set / new, 1 = HeadOfficeCreated, 2 = UserCreated, 3 = UserCommunicated.
        /// </summary>
        public int UserCreatedStatus { get; set; }
    }
}
