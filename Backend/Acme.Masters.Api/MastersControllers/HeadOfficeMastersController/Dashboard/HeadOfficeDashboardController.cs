/*  Class Name      : HeadOfficeDashboardController.cs
 *  Purpose         : Head Office Home dashboard endpoints (filters, summary)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Common;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard;
using Acme.Masters.Application.Features.HeadOfficeMasters.Dashboard;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.Dashboard
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/hodashboard")]
    public class HeadOfficeDashboardController : ControllerBase
    {
        private readonly HeadOfficeDashboardSystem _dashboard;

        public HeadOfficeDashboardController(HeadOfficeDashboardSystem dashboard)
        {
            _dashboard = dashboard;
        }

        [HttpGet("filters")]
        public async Task<IActionResult> FetchDashboardFilters(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _dashboard.FetchDashboardFilters(office, cancellationToken));
        }

        [HttpGet("summary")]
        public async Task<IActionResult> FetchDashboardSummary([FromQuery] DashboardSummaryRequest request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _dashboard.FetchDashboardSummary(office, request, cancellationToken));
        }

        private async Task<(HeadOfficeConnection? Office, IActionResult? Error)> ResolveOffice(CancellationToken cancellationToken)
        {
            string? scope = User.FindHeadOfficeScope();
            if (scope == null)
            {
                return (null, StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = MessageRender.GetMessage(MessageCatalog.Master.Dashboard.DASHBOARD_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _dashboard.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
