/*  Class Name      : RightRepository.cs
 *  Purpose         : User Rights load and UpdateUserRights on the portal or one head office database
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Acme.Auth.Application.Abstractions.Repositories;
using Acme.Auth.Application.Dtos;
using Acme.Auth.Domain.Entities;
using Acme.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Auth.Infrastructure.Repositories
{
    public class RightRepository : IRightRepository
    {
        private readonly AuthDbContext _portal;
        private readonly IConfiguration _configuration;

        public RightRepository(AuthDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        #region Helper Methods
        // office == null -> portal context (scoped by DI, not disposed here).
        // Otherwise a short-lived context for that head office database.
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
        #endregion

        public Task<IReadOnlyList<RightsRoleItemDto>> GetUsersType(HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run<IReadOnlyList<RightsRoleItemDto>>(office, async context =>
            {
                // Accessibility stays 0 -> roles with ACCESSIBILITY = 0
                return await context.Roles
                    .AsNoTracking()
                    .Where(role => role.Accessibility == 0)
                    .OrderBy(role => role.UserRole)
                    .Select(role => new RightsRoleItemDto
                    {
                        UserRoleId = role.UserRoleId,
                        UserRole = role.UserRole
                    })
                    .ToListAsync(cancellationToken);
            });
        }

        public Task<IReadOnlyList<RightsModuleDto>> GetAllUserRights(int roleId, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run<IReadOnlyList<RightsModuleDto>>(office, async context =>
            {
                List<RightsActivity> activities = await context.RightsActivities
                    .AsNoTracking()
                    .Where(row => row.Accessibility == 0)
                    .OrderBy(row => row.ModuleOrder)
                    .ThenBy(row => row.ActivityOrder)
                    .ToListAsync(cancellationToken);

                HashSet<string> allowed = (await context.RightsUsers
                    .AsNoTracking()
                    .Where(row => row.RoleId == roleId && row.Allow == 1)
                    .Select(row => row.ModuleCode + "\u0001" + row.ActivityCode)
                    .ToListAsync(cancellationToken))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                List<RightsModuleDto> modules = new List<RightsModuleDto>();
                foreach (IGrouping<string, RightsActivity> group in activities.GroupBy(row => row.ModuleCode))
                {
                    RightsActivity first = group.First();
                    modules.Add(new RightsModuleDto
                    {
                        ModuleCode = first.ModuleCode,
                        Module = first.Module,
                        Activities = group.Select(row => new RightsActivityItemDto
                        {
                            ActivityCode = row.ActivityCode,
                            Activity = row.Activity,
                            Allow = allowed.Contains(row.ModuleCode + "\u0001" + row.ActivityCode)
                        }).ToList()
                    });
                }

                return modules;
            });
        }

        public Task<RightsActionResult> UpdateUserRights(int roleId, IList<UpdateUserRightsItem> rights, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run(office, async context =>
            {
                if (roleId <= 0)
                {
                    return new RightsActionResult { Success = false, Message = "Role is required." };
                }

                bool roleExists = await context.Roles.AnyAsync(role => role.UserRoleId == roleId, cancellationToken);
                if (!roleExists)
                {
                    return new RightsActionResult { Success = false, Message = "User Role not found." };
                }

                if (rights == null || rights.Count == 0)
                {
                    return new RightsActionResult { Success = true, Message = "User Rights Saved." };
                }

                foreach (UpdateUserRightsItem item in rights)
                {
                    string moduleCode = (item.ModuleCode ?? string.Empty).Trim();
                    string activityCode = (item.ActivityCode ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(moduleCode) || string.IsNullOrWhiteSpace(activityCode))
                    {
                        continue;
                    }

                    RightsUser? existing = await context.RightsUsers.FirstOrDefaultAsync(
                        row => row.RoleId == roleId
                            && row.ModuleCode == moduleCode
                            && row.ActivityCode == activityCode,
                        cancellationToken);

                    if (item.Allow && existing == null)
                    {
                        context.RightsUsers.Add(new RightsUser
                        {
                            ModuleCode = moduleCode,
                            ActivityCode = activityCode,
                            RoleId = roleId,
                            Allow = 1
                        });
                    }
                    else if (!item.Allow && existing != null)
                    {
                        context.RightsUsers.Remove(existing);
                    }
                    else if (item.Allow && existing != null && existing.Allow != 1)
                    {
                        existing.Allow = 1;
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
                return new RightsActionResult { Success = true, Message = "User Rights Saved." };
            });
        }
    }
}
