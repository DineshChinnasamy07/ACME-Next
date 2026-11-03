/*  Class Name      : IHeadOfficeTenantWriter.cs
 *  Purpose         : Dual-write Head Office / status / admin user into HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Abstractions.Services
{
    public interface IHeadOfficeTenantWriter
    {
        Task<HeadOfficeActionResult> SaveHeadOfficeDetails(HeadOffice portalOffice, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> UpdateOfficeStatus(HeadOffice portalOffice, int status, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> EnsureHeadOfficeAdminUser(HeadOffice portalOffice, CancellationToken cancellationToken);
    }
}
