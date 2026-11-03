/*  Class Name      : IHeadOfficeDatabaseRestorer.cs
 *  Purpose         : Create/restore physical HO tenant database (Phase C)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using Bosco.Organization.Application.Dtos;

namespace Bosco.Organization.Application.Abstractions.Services
{
    public interface IHeadOfficeDatabaseRestorer
    {
        Task<HeadOfficeActionResult> RestoreACPERPdatabase(string hostName, string databaseName, string userName, string password, CancellationToken cancellationToken);
    }
}
