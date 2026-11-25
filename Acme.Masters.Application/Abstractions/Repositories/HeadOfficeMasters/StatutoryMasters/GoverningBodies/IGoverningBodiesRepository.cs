/*  Class Name      : IGoverningBodiesRepository.cs
 *  Purpose         : Governing Bodies (MASTER_EXECUTIVE_COMMITTEE) data access on the HO tenant database
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.GoverningBodies
{
    public interface IGoverningBodiesRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<GoverningBodyListItemDto>> FetchExecutiveMemberDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<GoverningBodyDetailDto?> FillExecutiveMemberDetails(HeadOfficeConnection office, int executiveId, CancellationToken cancellationToken);

        Task<GoverningBodyActionResult> SaveExecutiveMemberDetails(HeadOfficeConnection office, GoverningBodySaveRequest request, CancellationToken cancellationToken);

        Task<GoverningBodyActionResult> DeleteExecutiveMemberDetails(HeadOfficeConnection office, int executiveId, CancellationToken cancellationToken);
    }
}
