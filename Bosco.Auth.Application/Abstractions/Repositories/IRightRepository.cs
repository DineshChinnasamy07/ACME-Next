/*  Class Name      : IRightRepository.cs
 *  Purpose         : Portal rights_user / rights_activities used by RightSystem
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Auth.Application.Dtos;

namespace Bosco.Auth.Application.Abstractions.Repositories
{
    public interface IRightRepository
    {
        Task<IReadOnlyList<RightsRoleItemDto>> GetUsersType(CancellationToken cancellationToken);

        Task<IReadOnlyList<RightsModuleDto>> GetAllUserRights(int roleId, CancellationToken cancellationToken);

        Task<RightsActionResult> UpdateUserRights(int roleId, IList<UpdateUserRightsItem> rights, CancellationToken cancellationToken);
    }
}
