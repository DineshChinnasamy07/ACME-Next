/*  Class Name      : IBranchOfficeRepository.cs
 *  Purpose         : Portal branch_office access for BranchOfficeSystem
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Abstractions.Repositories
{
    public interface IBranchOfficeRepository
    {
        Task<IReadOnlyList<BranchOfficeListItemDto>> FetchBranchOfficeDetails(CancellationToken cancellationToken);

        Task<BranchOffice?> FillBranchOfficeDetails(int branchOfficeId, CancellationToken cancellationToken);

        Task<BranchOfficeActionResult> SaveBranchOfficeDetails(BranchOfficeSaveRequest request, int modifiedBy, CancellationToken cancellationToken);
    }
}
