/*  Class Name      : IHeadOfficeTenantWriter.cs
 *  Purpose         : Dual-write Head Office / status / admin user into HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using Acme.Organization.Application.Dtos;
using Acme.Organization.Domain.Entities;

namespace Acme.Organization.Application.Abstractions.Services
{
    public interface IHeadOfficeTenantWriter
    {
        Task<HeadOfficeActionResult> SaveHeadOfficeDetails(HeadOffice portalOffice, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> UpdateOfficeStatus(HeadOffice portalOffice, int status, CancellationToken cancellationToken);

        Task<HeadOfficeActionResult> EnsureHeadOfficeAdminUser(HeadOffice portalOffice, CancellationToken cancellationToken);
    }
}
