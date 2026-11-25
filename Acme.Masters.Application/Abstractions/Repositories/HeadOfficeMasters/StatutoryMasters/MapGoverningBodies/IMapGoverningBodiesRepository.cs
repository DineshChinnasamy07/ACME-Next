/*  Class Name      : IMapGoverningBodiesRepository.cs
 *  Purpose         : Map Governing Bodies (EXECUTIVE_LEGAL_ENTITY) data access on the HO tenant database
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies
{
    public interface IMapGoverningBodiesRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<LegalEntityOptionDto>> LoadLegalEntityLookup(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<IReadOnlyList<GoverningBodyMappingRowDto>?> FetchGoverningMembersByLegalEntity(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken);

        Task<GoverningBodyMappingActionResult> MapGoverningMembers(HeadOfficeConnection office, GoverningBodyMappingSaveRequest request, CancellationToken cancellationToken);

        Task<GoverningBodyMappingActionResult> DeleteMappedGoverningMember(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken);
    }
}
