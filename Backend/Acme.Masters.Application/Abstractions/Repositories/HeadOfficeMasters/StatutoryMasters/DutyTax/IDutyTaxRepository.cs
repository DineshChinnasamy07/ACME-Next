/*  Class Name      : IDutyTaxRepository.cs
 *  Purpose         : Data access contract for the Head Office TDS Duty / Tax type master
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.DutyTax
{
    public interface IDutyTaxRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<DutyTaxListItemDto>> FetchDutyTaxDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<DutyTaxListItemDto?> FillDutyTaxDetails(HeadOfficeConnection office, int tdsDutyTaxTypeId, CancellationToken cancellationToken);

        Task<DutyTaxActionResult> SaveDutyTaxDetails(HeadOfficeConnection office, DutyTaxSaveRequest request, CancellationToken cancellationToken);

        Task<DutyTaxActionResult> DeleteDutyTaxDetails(HeadOfficeConnection office, int tdsDutyTaxTypeId, CancellationToken cancellationToken);
    }
}
