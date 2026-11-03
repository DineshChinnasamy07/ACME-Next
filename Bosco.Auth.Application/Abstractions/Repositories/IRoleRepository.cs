/*  Class Name      : IRoleRepository.cs
 *  Purpose         : Portal user_role CRUD used by UserRoleSystem
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Bosco.Auth.Application.Dtos;

namespace Bosco.Auth.Application.Abstractions.Repositories
{
    public interface IRoleRepository
    {
        Task<IReadOnlyList<RoleListItemDto>> FetchUserRoleDetails(CancellationToken cancellationToken);

        Task<RoleSaveRequest?> FetchUserDetailsbyId(int userRoleId, CancellationToken cancellationToken);

        Task<RoleActionResult> SaveUserRoles(RoleSaveRequest request, int accessibility, CancellationToken cancellationToken);

        Task<RoleActionResult> DeleteUserRole(int userRoleId, CancellationToken cancellationToken);
    }
}
