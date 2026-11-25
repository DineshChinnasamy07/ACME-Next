/*  Class Name      : IDeducteeTypeRepository.cs
 *  Purpose         : Data access contract for the Head Office TDS Deductee Type master
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.DeducteeType
{
    public interface IDeducteeTypeRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<DeducteeTypeListItemDto>> FetchDeducteeTypeDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<DeducteeTypeListItemDto?> FillDeducteeTypeDetails(HeadOfficeConnection office, int deducteeTypeId, CancellationToken cancellationToken);

        Task<DeducteeTypeActionResult> SaveDeducteeTypeDetails(HeadOfficeConnection office, DeducteeTypeSaveRequest request, CancellationToken cancellationToken);

        Task<DeducteeTypeActionResult> DeleteDeducteeTypeDetails(HeadOfficeConnection office, int deducteeTypeId, CancellationToken cancellationToken);
    }
}
