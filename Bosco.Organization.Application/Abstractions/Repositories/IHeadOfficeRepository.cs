/*  Class Name      : IHeadOfficeRepository.cs
 *  Purpose         : Portal head_office access for HeadOfficeSystem
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Abstractions.Repositories
{
    public interface IHeadOfficeRepository
    {
        Task<IReadOnlyList<HeadOfficeListItemDto>> FetchHeadOfficeDetails(CancellationToken cancellationToken);

        Task<HeadOffice?> HeadOfficeDetailById(int headOfficeId, CancellationToken cancellationToken);

        Task<HeadOffice?> HeadOfficeDetailByCode(string headOfficeCode, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> SaveHeadOfficeDetails(HeadOfficeSaveRequest request, int modifiedBy, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> UpdateOfficeStatus(string headOfficeCode, int status, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> DeleteHeadOfficeDetails(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<ActiveHeadOfficeDto>> FetchActiveHeadOfficeDetails(CancellationToken cancellationToken);

        Task<IReadOnlyList<HeadOfficeTypeDto>> FetchHeadOfficeType(CancellationToken cancellationToken);

        Task<IReadOnlyList<CountryDto>> FetchCountry(CancellationToken cancellationToken);

        Task<IReadOnlyList<StateDto>> FetchStateByCountry(int countryId, CancellationToken cancellationToken);

        Task<IReadOnlyList<BranchByHeadOfficeDto>> FetchBranchByHeadOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> UpdateDatabase(string headOfficeCode, UpdateDatabaseRequest request, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> UpdateUserCreatedStatus(string headOfficeCode, int userCreatedStatus, CancellationToken cancellationToken);
    }
}
