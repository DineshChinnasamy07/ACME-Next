/*  Class Name      : RoleRepository.cs
 *  Purpose         : user_role list, save, and delete on the portal or one head office database
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Acme.Auth.Application.Abstractions.Repositories;
using Acme.Auth.Application.Dtos;
using Acme.Auth.Domain.Entities;
using Acme.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Auth.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AuthDbContext _portal;
        private readonly IConfiguration _configuration;

        public RoleRepository(AuthDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        #region Helper Methode
        // office == null runs on the portal context (scoped by DI, never disposed here).
        // Otherwise runs on a short-lived context for that head office database.
        private async Task<T> Run<T>(HeadOffice? office, Func<AuthDbContext, Task<T>> action)
        {
            if (office == null)
            {
                return await action(_portal);
            }

            string connectionString = HeadOfficeConnection.Build(office, _configuration);
            DbContextOptions<AuthDbContext> options = new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(connectionString).Options;
            using (AuthDbContext headOffice = new AuthDbContext(options))
            {
                return await action(headOffice);
            }
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
        #endregion

        public Task<IReadOnlyList<RoleListItemDto>> FetchUserRoleDetails(HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run<IReadOnlyList<RoleListItemDto>>(office, async context =>
            {
                List<RoleListItemDto> rows = await context.Roles
                    .AsNoTracking()
                    .Where(role => role.UserRole != null && role.Accessibility == 0)
                    .OrderBy(role => role.UserRole)
                    .Select(role => new RoleListItemDto
                    {
                        UserRoleId = role.UserRoleId,
                        Name = role.UserRole,
                        UserCount = context.Users.Count(user => user.RoleId == role.UserRoleId)
                    })
                    .ToListAsync(cancellationToken);

                foreach (RoleListItemDto row in rows)
                {
                    row.IsSystem = row.UserRoleId == 1 || row.UserRoleId == 2
                        || string.Equals(row.Name.Trim(), "Admin", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(row.Name.Trim(), "Branch Admin", StringComparison.OrdinalIgnoreCase);
                }

                return rows;
            });
        }

        public Task<RoleSaveRequest?> FetchUserDetailsbyId(int userRoleId, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run<RoleSaveRequest?>(office, context => context.Roles
                .AsNoTracking()
                .Where(role => role.UserRoleId == userRoleId)
                .Select(role => new RoleSaveRequest
                {
                    UserRoleId = role.UserRoleId,
                    UserRoleName = role.UserRole
                })
                .FirstOrDefaultAsync(cancellationToken));
        }

        public Task<RoleActionResult> SaveUserRoles(RoleSaveRequest request, int accessibility, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run(office, async context =>
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

                bool duplicate = await context.Roles.AnyAsync(
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
                    context.Roles.Add(role);
                    await context.SaveChangesAsync(cancellationToken);
                    return new RoleActionResult
                    {
                        Success = true,
                        Message = "Role Saved",
                        UserRoleId = role.UserRoleId
                    };
                }

                Role? existing = await context.Roles.FirstOrDefaultAsync(role => role.UserRoleId == request.UserRoleId, cancellationToken);
                if (existing == null)
                {
                    return new RoleActionResult { Success = false, Message = "User Role not found." };
                }

                if (IsProtectedRole(existing))
                {
                    return new RoleActionResult { Success = false, Message = "Cannot edit Admin or Branch Admin." };
                }

                existing.UserRole = userRoleName;
                await context.SaveChangesAsync(cancellationToken);
                return new RoleActionResult
                {
                    Success = true,
                    Message = "Role Saved",
                    UserRoleId = existing.UserRoleId
                };
            });
        }

        public Task<RoleActionResult> DeleteUserRole(int userRoleId, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run(office, async context =>
            {
                Role? existing = await context.Roles.FirstOrDefaultAsync(role => role.UserRoleId == userRoleId, cancellationToken);
                if (existing == null)
                {
                    return new RoleActionResult { Success = false, Message = "User Role not found." };
                }

                if (IsProtectedRole(existing))
                {
                    return new RoleActionResult { Success = false, Message = "Cannot delete Admin or Branch Admin." };
                }

                bool usedByUser = await context.Users.AnyAsync(user => user.RoleId == userRoleId, cancellationToken);
                if (usedByUser)
                {
                    return new RoleActionResult { Success = false, Message = "Cannot delete this User Role because it is assigned to a user." };
                }

                context.Roles.Remove(existing);
                try
                {
                    await context.SaveChangesAsync(cancellationToken);
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
            });
        }
    }
}
