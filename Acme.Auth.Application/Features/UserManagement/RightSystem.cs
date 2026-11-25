/*  Class Name      : RightSystem.cs
 *  Purpose         : User Rights on the portal database or one head office database
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Acme.Auth.Application.Abstractions.Repositories;
using Acme.Auth.Application.Dtos;
using Acme.Auth.Domain.Entities;

namespace Acme.Auth.Application.Features.UserManagement
{
    public class RightSystem
    {
        private const string PortalCode = "portal";

        private readonly IRightRepository _rights;
        private readonly IHeadOfficeRepository _headOffices;

        public RightSystem(IRightRepository rights, IHeadOfficeRepository headOffices)
        {
            _rights = rights;
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

        public async Task<IReadOnlyList<RightsRoleItemDto>> GetUsersType(string headOfficeCode, CancellationToken cancellationToken)
        {
            var target = await ResolveOffice(headOfficeCode, cancellationToken);
            if (!target.Found)
            {
                return new List<RightsRoleItemDto>();
            }

            return await _rights.GetUsersType(target.Office, cancellationToken);
        }

        public async Task<IReadOnlyList<RightsModuleDto>> GetAllUserRights(int roleId, string headOfficeCode, CancellationToken cancellationToken)
        {
            if (roleId <= 0)
            {
                return Array.Empty<RightsModuleDto>();
            }

            var target = await ResolveOffice(headOfficeCode, cancellationToken);
            if (!target.Found)
            {
                return Array.Empty<RightsModuleDto>();
            }

            return await _rights.GetAllUserRights(roleId, target.Office, cancellationToken);
        }

        public async Task<RightsActionResult> UpdateUserRights(UpdateUserRightsRequest request, string headOfficeCode, CancellationToken cancellationToken)
        {
            if (request == null || request.RoleId <= 0)
            {
                return new RightsActionResult { Success = false, Message = "Role is required." };
            }

            var target = await ResolveOffice(headOfficeCode, cancellationToken);
            if (!target.Found)
            {
                return new RightsActionResult { Success = false, Message = "Invalid Head Office Code" };
            }

            return await _rights.UpdateUserRights(request.RoleId, request.Rights ?? new List<UpdateUserRightsItem>(), target.Office, cancellationToken);
        }
    }
}
