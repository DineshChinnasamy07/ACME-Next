/*  Class Name      : IBranchOfficeTenantWriter.cs
 *  Purpose         : Dual-write Branch Office row, status, and delete into HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 10-Nov-2026
 */
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Abstractions.Services
{
    public interface IBranchOfficeTenantWriter
    {
        Task<BranchOfficeActionResult> SaveBranchOfficeDetails(HeadOffice headOffice, BranchOffice portalBranch, CancellationToken cancellationToken);

        Task<BranchOfficeActionResult> UpdateOfficeStatus(HeadOffice headOffice, BranchOffice portalBranch, int status, CancellationToken cancellationToken);

        Task<BranchOfficeActionResult> DeleteBranchOfficeDetails(HeadOffice headOffice, string branchOfficeCode, CancellationToken cancellationToken);
    }
}
