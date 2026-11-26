/*  Class Name      : ITdsPolicyRepository.cs
 *  Purpose         : Head Office Statutory Masters - TDS Policy data access contract
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public interface ITdsPolicyRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<TdsPolicyListItemDto>> FetchTdsPolicyDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<TdsPolicyLookupsDto> FetchTdsPolicyLookups(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<TdsPolicyDetailDto?> FillTdsPolicyDetails(HeadOfficeConnection office, int tdsPolicyId, CancellationToken cancellationToken);

        Task<TdsPolicyActionResult> SaveTdsPolicyDetails(HeadOfficeConnection office, TdsPolicySaveRequest request, CancellationToken cancellationToken);

        Task<TdsPolicyActionResult> DeleteTdsPolicyDetails(HeadOfficeConnection office, int tdsPolicyId, CancellationToken cancellationToken);
    }
}
