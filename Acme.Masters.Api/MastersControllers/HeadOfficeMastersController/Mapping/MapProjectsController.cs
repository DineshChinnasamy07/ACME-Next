/*  Class Name      : MapProjectsController.cs
 *  Purpose         : Head Office Mapping - Project Mapping endpoints (lookups, fill, save)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Common;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects;
using Acme.Masters.Application.Features.HeadOfficeMasters.Mapping.MapProjects;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.Mapping
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapprojects")]
    public class MapProjectsController : ControllerBase
    {
        private readonly MapProjectsSystem _mappings;

        public MapProjectsController(MapProjectsSystem mappings)
        {
            _mappings = mappings;
        }

        [HttpGet("lookups")]
        public async Task<IActionResult> FetchMappingLookups(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _mappings.FetchMappingLookups(office, cancellationToken));
        }

        [HttpGet("{projectId:int}")]
        public async Task<IActionResult> FillProjectMapping(int projectId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            ProjectMappingDetailDto? detail = await _mappings.FillProjectMapping(office, projectId, cancellationToken);
            if (detail == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_NOT_FOUND) });
            }

            return Ok(detail);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProjectMapping([FromBody] ProjectMappingSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            ProjectMappingActionResult result = await _mappings.SaveProjectMapping(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message, rowIndex = result.RowIndex });
            return Ok(result);
        }

        private async Task<(HeadOfficeConnection? Office, IActionResult? Error)> ResolveOffice(CancellationToken cancellationToken)
        {
            string? scope = User.FindHeadOfficeScope();
            if (scope == null)
            {
                return (null, StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _mappings.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
