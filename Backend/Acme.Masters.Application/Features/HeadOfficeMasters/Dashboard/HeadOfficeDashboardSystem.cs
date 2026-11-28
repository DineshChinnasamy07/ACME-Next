/*  Class Name      : HeadOfficeDashboardSystem.cs
 *  Purpose         : Head Office dashboard business entry point (legacy HomeLogin Quick View + HomeDashboard Data Status)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.Dashboard;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.Dashboard
{
    public class HeadOfficeDashboardSystem
    {
        private readonly IHeadOfficeDashboardRepository _dashboard;

        public HeadOfficeDashboardSystem(IHeadOfficeDashboardRepository dashboard)
        {
            _dashboard = dashboard;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _dashboard.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<DashboardFiltersDto> FetchDashboardFilters(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _dashboard.FetchDashboardFilters(office, cancellationToken);
        }

        public Task<DashboardSummaryDto> FetchDashboardSummary(HeadOfficeConnection office, DashboardSummaryRequest? request, CancellationToken cancellationToken)
        {
            DashboardSummaryRequest criteria = request ?? new DashboardSummaryRequest();
            criteria.BranchId = Math.Max(0, criteria.BranchId);
            criteria.ProjectId = Math.Max(0, criteria.ProjectId);
            if (criteria.PendingFrom.HasValue && criteria.PendingTo.HasValue && criteria.PendingFrom.Value > criteria.PendingTo.Value)
            {
                DateTime swap = criteria.PendingFrom.Value;
                criteria.PendingFrom = criteria.PendingTo;
                criteria.PendingTo = swap;
            }

            return _dashboard.FetchDashboardSummary(office, criteria, cancellationToken);
        }
    }
}
