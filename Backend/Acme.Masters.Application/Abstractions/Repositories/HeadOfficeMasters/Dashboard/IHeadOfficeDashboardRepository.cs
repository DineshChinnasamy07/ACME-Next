/*  Class Name      : IHeadOfficeDashboardRepository.cs
 *  Purpose         : Head Office dashboard data access on the HO tenant database
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.Dashboard
{
    public interface IHeadOfficeDashboardRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<DashboardFiltersDto> FetchDashboardFilters(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<DashboardSummaryDto> FetchDashboardSummary(HeadOfficeConnection office, DashboardSummaryRequest request, CancellationToken cancellationToken);
    }
}
