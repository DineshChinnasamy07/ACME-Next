/*  Class Name      : UserRoleSystem.cs
 *  Purpose         : User Role CRUD on the portal database or one head office database
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Acme.Auth.Application.Abstractions.Repositories;
using Acme.Auth.Application.Dtos;
using Acme.Auth.Domain.Entities;

namespace Acme.Auth.Application.Features.UserManagement
{
    public class UserRoleSystem
    {
        private const string PortalCode = "portal";

        private readonly IRoleRepository _roles;
        private readonly IHeadOfficeRepository _headOffices;

        public UserRoleSystem(IRoleRepository roles, IHeadOfficeRepository headOffices)
        {
            _roles = roles;
            _headOffices = headOffices;
        }

        // "" or "portal" -> portal database (Office == null). Otherwise that head office database.
        private async Task<(bool Found, HeadOffice? Office)> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            string code = headOfficeCode == null ? string.Empty : headOfficeCode.Trim();
            if (code.Length == 0 || string.Equals(code, PortalCode, StringComparison.OrdinalIgnoreCase))
            {
                return (true, null);
            }

            HeadOffice? office = await _headOffices.FetchDatabase(code, cancellationToken);
            if (office == null || string.IsNullOrWhiteSpace(office.HostName) || string.IsNullOrWhiteSpace(office.DbName))
            {
                return (false, null);
            }

            return (true, office);
        }

        private static RoleActionResult InvalidOffice()
        {
            return new RoleActionResult { Success = false, Message = "Invalid Head Office Code" };
        }

        public async Task<IReadOnlyList<RoleListItemDto>> FetchUserRoleDetails(string headOfficeCode, CancellationToken cancellationToken)
        {
            var target = await ResolveOffice(headOfficeCode, cancellationToken);
            if (!target.Found)
            {
                return new List<RoleListItemDto>();
            }

            return await _roles.FetchUserRoleDetails(target.Office, cancellationToken);
        }

        public async Task<RoleSaveRequest?> FillSaveUserRole(int userRoleId, string headOfficeCode, CancellationToken cancellationToken)
        {
            var target = await ResolveOffice(headOfficeCode, cancellationToken);
            if (!target.Found)
            {
                return null;
            }

            return await _roles.FetchUserDetailsbyId(userRoleId, target.Office, cancellationToken);
        }

        public async Task<RoleActionResult> SaveUserRoles(RoleSaveRequest request, string headOfficeCode, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new RoleActionResult { Success = false, Message = "Role Name is required" };
            }

            string userRoleName = (request.UserRoleName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(userRoleName))
            {
                return new RoleActionResult { Success = false, Message = "Role Name is required" };
            }

            var target = await ResolveOffice(headOfficeCode, cancellationToken);
            if (!target.Found)
            {
                return InvalidOffice();
            }

            // Accessibility.Both = 0 for portal and head office roles
            return await _roles.SaveUserRoles(request, 0, target.Office, cancellationToken);
        }

        public async Task<RoleActionResult> DeleteUserRole(int userRoleId, string headOfficeCode, CancellationToken cancellationToken)
        {
            if (userRoleId <= 0)
            {
                return new RoleActionResult { Success = false, Message = "User Role not found." };
            }

            var target = await ResolveOffice(headOfficeCode, cancellationToken);
            if (!target.Found)
            {
                return InvalidOffice();
            }

            return await _roles.DeleteUserRole(userRoleId, target.Office, cancellationToken);
        }
    }
}
