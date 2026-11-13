/*  Class Name      : IRightRepository.cs
 *  Purpose         : rights_user / rights_activities used by RightSystem (portal or one head office database)
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Domain.Entities;

namespace Bosco.Auth.Application.Abstractions.Repositories
{
    public interface IRightRepository
    {
        // office == null runs on the portal database; otherwise on that head office database.
        Task<IReadOnlyList<RightsRoleItemDto>> GetUsersType(HeadOffice? office, CancellationToken cancellationToken);

        Task<IReadOnlyList<RightsModuleDto>> GetAllUserRights(int roleId, HeadOffice? office, CancellationToken cancellationToken);

        Task<RightsActionResult> UpdateUserRights(int roleId, IList<UpdateUserRightsItem> rights, HeadOffice? office, CancellationToken cancellationToken);
    }
}
