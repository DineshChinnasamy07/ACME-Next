/*  Class Name      : UserRoleSystem.cs
 *  Purpose         : Portal User Role CRUD matching ACCPPORTAL UserRoleSystem
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Auth.Application.Abstractions.Repositories;
using Bosco.Auth.Application.Dtos;

namespace Bosco.Auth.Application.Features.UserManagement
{
    public class UserRoleSystem
    {
        private readonly IRoleRepository _roles;

        public UserRoleSystem(IRoleRepository roles)
        {
            _roles = roles;
        }

        public Task<IReadOnlyList<RoleListItemDto>> FetchUserRoleDetails(CancellationToken cancellationToken)
        {
            return _roles.FetchUserRoleDetails(cancellationToken);
        }

        public Task<RoleSaveRequest?> FillSaveUserRole(int userRoleId, CancellationToken cancellationToken)
        {
            return _roles.FetchUserDetailsbyId(userRoleId, cancellationToken);
        }

        public Task<RoleActionResult> SaveUserRoles(RoleSaveRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new RoleActionResult { Success = false, Message = "Role Name is required" });
            }

            string userRoleName = (request.UserRoleName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(userRoleName))
            {
                return Task.FromResult(new RoleActionResult { Success = false, Message = "Role Name is required" });
            }

            // Portal admin saves with Accessibility.Both = 0
            return _roles.SaveUserRoles(request, 0, cancellationToken);
        }

        public Task<RoleActionResult> DeleteUserRole(int userRoleId, CancellationToken cancellationToken)
        {
            if (userRoleId <= 0)
            {
                return Task.FromResult(new RoleActionResult { Success = false, Message = "User Role not found." });
            }

            return _roles.DeleteUserRole(userRoleId, cancellationToken);
        }
    }
}
