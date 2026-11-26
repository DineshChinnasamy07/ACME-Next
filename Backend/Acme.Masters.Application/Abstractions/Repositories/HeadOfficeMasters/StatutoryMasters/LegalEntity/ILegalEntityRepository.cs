/*  Class Name      : ILegalEntityRepository.cs
 *  Purpose         : Legal Entity (MASTER_INSTI_PERFERENCE) data access on the HO tenant database
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public interface ILegalEntityRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<LegalEntityListItemDto>> FetchLegalEntity(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<LegalEntityDetailDto?> FillLegalEntityDetails(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken);

        Task<LegalEntityActionResult> SaveLegalEntityDetails(HeadOfficeConnection office, LegalEntitySaveRequest request, CancellationToken cancellationToken);

        Task<LegalEntityActionResult> DeleteLegalEntityDetails(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken);
    }
}
