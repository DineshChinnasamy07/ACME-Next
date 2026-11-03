/*  Class Name      : RightSystem.cs
 *  Purpose         : Portal User Rights matching ACCPPORTAL RightSystem
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Auth.Application.Abstractions.Repositories;
using Bosco.Auth.Application.Dtos;

namespace Bosco.Auth.Application.Features.UserManagement
{
    public class RightSystem
    {
        private readonly IRightRepository _rights;

        public RightSystem(IRightRepository rights)
        {
            _rights = rights;
        }

        public Task<IReadOnlyList<RightsRoleItemDto>> GetUsersType(CancellationToken cancellationToken)
        {
            return _rights.GetUsersType(cancellationToken);
        }

        public Task<IReadOnlyList<RightsModuleDto>> GetAllUserRights(int roleId, CancellationToken cancellationToken)
        {
            if (roleId <= 0)
            {
                return Task.FromResult<IReadOnlyList<RightsModuleDto>>(Array.Empty<RightsModuleDto>());
            }

            return _rights.GetAllUserRights(roleId, cancellationToken);
        }

        public Task<RightsActionResult> UpdateUserRights(UpdateUserRightsRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.RoleId <= 0)
            {
                return Task.FromResult(new RightsActionResult { Success = false, Message = "Role is required." });
            }

            return _rights.UpdateUserRights(request.RoleId, request.Rights ?? new List<UpdateUserRightsItem>(), cancellationToken);
        }
    }
}
