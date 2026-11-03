/*  Class Name      : RoleRepository.cs
 *  Purpose         : Portal user_role list, save, and delete
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Bosco.Auth.Application.Abstractions.Repositories;
using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Domain.Entities;
using Bosco.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bosco.Auth.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AuthDbContext _portal;

        public RoleRepository(AuthDbContext portal)
        {
            _portal = portal;
        }

        public async Task<IReadOnlyList<RoleListItemDto>> FetchUserRoleDetails(CancellationToken cancellationToken)
        {
            return await _portal.Roles
                .AsNoTracking()
                .Where(role => role.UserRole != null && role.Accessibility == 0)
                .OrderBy(role => role.UserRole)
                .Select(role => new RoleListItemDto
                {
                    UserRoleId = role.UserRoleId,
                    Name = role.UserRole
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<RoleSaveRequest?> FetchUserDetailsbyId(int userRoleId, CancellationToken cancellationToken)
        {
            return await _portal.Roles
                .AsNoTracking()
                .Where(role => role.UserRoleId == userRoleId)
                .Select(role => new RoleSaveRequest
                {
                    UserRoleId = role.UserRoleId,
                    UserRoleName = role.UserRole
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<RoleActionResult> SaveUserRoles(RoleSaveRequest request, int accessibility, CancellationToken cancellationToken)
        {
            string userRoleName = (request.UserRoleName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(userRoleName))
            {
                return new RoleActionResult { Success = false, Message = "Role Name is required" };
            }

            if (userRoleName.Length > 30)
            {
                return new RoleActionResult { Success = false, Message = "Role Name is required" };
            }

            bool duplicate = await _portal.Roles.AnyAsync(
                role => role.UserRole == userRoleName && role.UserRoleId != request.UserRoleId,
                cancellationToken);
            if (duplicate)
            {
                return new RoleActionResult { Success = false, Message = "Role name already exists." };
            }

            if (request.UserRoleId == 0)
            {
                Role role = new Role
                {
                    UserRole = userRoleName,
                    Accessibility = accessibility
                };
                _portal.Roles.Add(role);
                await _portal.SaveChangesAsync(cancellationToken);
                return new RoleActionResult
                {
                    Success = true,
                    Message = "Role Saved",
                    UserRoleId = role.UserRoleId
                };
            }

            Role? existing = await _portal.Roles.FirstOrDefaultAsync(role => role.UserRoleId == request.UserRoleId, cancellationToken);
            if (existing == null)
            {
                return new RoleActionResult { Success = false, Message = "User Role not found." };
            }

            if (IsProtectedRole(existing))
            {
                return new RoleActionResult { Success = false, Message = "Cannot edit Admin or Branch Admin." };
            }

            existing.UserRole = userRoleName;
            await _portal.SaveChangesAsync(cancellationToken);
            return new RoleActionResult
            {
                Success = true,
                Message = "Role Saved",
                UserRoleId = existing.UserRoleId
            };
        }

        public async Task<RoleActionResult> DeleteUserRole(int userRoleId, CancellationToken cancellationToken)
        {
            Role? existing = await _portal.Roles.FirstOrDefaultAsync(role => role.UserRoleId == userRoleId, cancellationToken);
            if (existing == null)
            {
                return new RoleActionResult { Success = false, Message = "User Role not found." };
            }

            if (IsProtectedRole(existing))
            {
                return new RoleActionResult { Success = false, Message = "Cannot delete Admin or Branch Admin." };
            }

            bool usedByUser = await _portal.Users.AnyAsync(user => user.RoleId == userRoleId, cancellationToken);
            if (usedByUser)
            {
                return new RoleActionResult { Success = false, Message = "Cannot delete this User Role because it is assigned to a user." };
            }

            _portal.Roles.Remove(existing);
            try
            {
                await _portal.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return new RoleActionResult { Success = false, Message = "Cannot delete this User Role because it is in use." };
            }

            return new RoleActionResult
            {
                Success = true,
                Message = "User Role is Deleted",
                UserRoleId = userRoleId
            };
        }

        private static bool IsProtectedRole(Role role)
        {
            if (role.UserRoleId == 1 || role.UserRoleId == 2)
            {
                return true;
            }

            string name = (role.UserRole ?? string.Empty).Trim();
            return string.Equals(name, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Branch Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
