/*  Class Name      : ITdsSectionRepository.cs
 *  Purpose         : Data access contract for the Head Office TDS Section master
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.TdsSection
{
    public interface ITdsSectionRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<TdsSectionListItemDto>> FetchTdsSectionDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<TdsSectionListItemDto?> FillTdsSectionDetails(HeadOfficeConnection office, int tdsSectionId, CancellationToken cancellationToken);

        Task<TdsSectionActionResult> SaveTdsSectionDetails(HeadOfficeConnection office, TdsSectionSaveRequest request, CancellationToken cancellationToken);

        Task<TdsSectionActionResult> DeleteTdsSectionDetails(HeadOfficeConnection office, int tdsSectionId, CancellationToken cancellationToken);
    }
}
