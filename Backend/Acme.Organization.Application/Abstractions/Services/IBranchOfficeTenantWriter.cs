/*  Class Name      : IBranchOfficeTenantWriter.cs
 *  Purpose         : Dual-write Branch Office row, status, and delete into HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 10-Nov-2026
 */
using Acme.Organization.Application.Dtos;
using Acme.Organization.Domain.Entities;

namespace Acme.Organization.Application.Abstractions.Services
{
    public interface IBranchOfficeTenantWriter
    {
        Task<BranchOfficeActionResult> SaveBranchOfficeDetails(HeadOffice headOffice, BranchOffice portalBranch, CancellationToken cancellationToken);

        Task<BranchOfficeActionResult> UpdateOfficeStatus(HeadOffice headOffice, BranchOffice portalBranch, int status, CancellationToken cancellationToken);

        Task<BranchOfficeActionResult> DeleteBranchOfficeDetails(HeadOffice headOffice, string branchOfficeCode, CancellationToken cancellationToken);

        Task<BranchOfficeActionResult> EnsureBranchOfficeAdminUser(HeadOffice headOffice, BranchOffice portalBranch, CancellationToken cancellationToken);
    }
}
