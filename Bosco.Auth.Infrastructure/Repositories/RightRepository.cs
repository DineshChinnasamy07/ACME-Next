/*  Class Name      : RightRepository.cs
 *  Purpose         : Portal User Rights load and UpdateUserRights
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Auth.Application.Abstractions.Repositories;
using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Domain.Entities;
using Bosco.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bosco.Auth.Infrastructure.Repositories
{
    public class RightRepository : IRightRepository
    {
        private readonly AuthDbContext _portal;

        public RightRepository(AuthDbContext portal)
        {
            _portal = portal;
        }

        public async Task<IReadOnlyList<RightsRoleItemDto>> GetUsersType(CancellationToken cancellationToken)
        {
            // Portal admin: Accessibility stays 0 → roles with ACCESSIBILITY = 0
            return await _portal.Roles
                .AsNoTracking()
                .Where(role => role.Accessibility == 0)
                .OrderBy(role => role.UserRole)
                .Select(role => new RightsRoleItemDto
                {
                    UserRoleId = role.UserRoleId,
                    UserRole = role.UserRole
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RightsModuleDto>> GetAllUserRights(int roleId, CancellationToken cancellationToken)
        {
            List<RightsActivity> activities = await _portal.RightsActivities
                .AsNoTracking()
                .Where(row => row.Accessibility == 0)
                .OrderBy(row => row.ModuleOrder)
                .ThenBy(row => row.ActivityOrder)
                .ToListAsync(cancellationToken);

            HashSet<string> allowed = (await _portal.RightsUsers
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
        }

        public async Task<RightsActionResult> UpdateUserRights(int roleId, IList<UpdateUserRightsItem> rights, CancellationToken cancellationToken)
        {
            if (roleId <= 0)
            {
                return new RightsActionResult { Success = false, Message = "Role is required." };
            }

            bool roleExists = await _portal.Roles.AnyAsync(role => role.UserRoleId == roleId, cancellationToken);
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

                RightsUser? existing = await _portal.RightsUsers.FirstOrDefaultAsync(
                    row => row.RoleId == roleId
                        && row.ModuleCode == moduleCode
                        && row.ActivityCode == activityCode,
                    cancellationToken);

                if (item.Allow && existing == null)
                {
                    _portal.RightsUsers.Add(new RightsUser
                    {
                        ModuleCode = moduleCode,
                        ActivityCode = activityCode,
                        RoleId = roleId,
                        Allow = 1
                    });
                }
                else if (!item.Allow && existing != null)
                {
                    _portal.RightsUsers.Remove(existing);
                }
                else if (item.Allow && existing != null && existing.Allow != 1)
                {
                    existing.Allow = 1;
                }
            }

            await _portal.SaveChangesAsync(cancellationToken);
            return new RightsActionResult { Success = true, Message = "User Rights Saved." };
        }
    }
}
