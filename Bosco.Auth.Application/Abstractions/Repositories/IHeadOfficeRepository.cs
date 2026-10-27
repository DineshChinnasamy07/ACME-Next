/*  Class Name      : IHeadOfficeRepository.cs
 *  Purpose         : FetchDatabase reads the portal head_office connection row
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Domain.Entities;

namespace Bosco.Auth.Application.Abstractions.Repositories
{
    public interface IHeadOfficeRepository
    {
        Task<HeadOffice?> FetchDatabase(string headOfficeCode, CancellationToken cancellationToken);
    }
}
