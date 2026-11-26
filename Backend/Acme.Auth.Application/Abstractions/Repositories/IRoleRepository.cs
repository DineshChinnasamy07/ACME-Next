/*  Class Name      : IRoleRepository.cs
 *  Purpose         : user_role CRUD used by UserRoleSystem (portal database or one head office database)
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Acme.Auth.Application.Dtos;
using Acme.Auth.Domain.Entities;

namespace Acme.Auth.Application.Abstractions.Repositories
{
    public interface IRoleRepository
    {
        // office == null runs on the portal database; otherwise on that head office database.
        Task<IReadOnlyList<RoleListItemDto>> FetchUserRoleDetails(HeadOffice? office, CancellationToken cancellationToken);

        Task<RoleSaveRequest?> FetchUserDetailsbyId(int userRoleId, HeadOffice? office, CancellationToken cancellationToken);

        Task<RoleActionResult> SaveUserRoles(RoleSaveRequest request, int accessibility, HeadOffice? office, CancellationToken cancellationToken);

        Task<RoleActionResult> DeleteUserRole(int userRoleId, HeadOffice? office, CancellationToken cancellationToken);
    }
}
