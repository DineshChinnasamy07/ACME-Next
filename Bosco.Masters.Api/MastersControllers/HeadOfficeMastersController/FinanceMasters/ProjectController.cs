using Bosco.Common;
using Bosco.Common.Security;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Masters.Api.MastersControllers.HeadOfficeMastersController.FinanceMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectSystem _projects;

        public ProjectController(ProjectSystem projects)
        {
            _projects = projects;
        }

        [HttpGet]
        public async Task<IActionResult> FetchProjectDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _projects.FetchProjectDetails(office, cancellationToken));
        }

        [HttpGet("divisions")]
        public async Task<IActionResult> FetchDivision(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _projects.FetchDivision(office, cancellationToken));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> FetchProjectCategories(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _projects.FetchProjectCategories(office, cancellationToken));
        }

        [HttpGet("societies")]
        public async Task<IActionResult> FetchSocietyNames(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _projects.FetchSocietyNames(office, cancellationToken));
        }

        [HttpGet("{projectId:int}")]
        public async Task<IActionResult> FillProjectDetails(int projectId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            MasterProject? row = await _projects.FillProjectDetails(office, projectId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_NOT_FOUND) });
            }

            return Ok(row);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProjectDetails([FromBody] ProjectSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            ProjectActionResult result = await _projects.SaveProjectDetails(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpDelete("{projectId:int}")]
        public async Task<IActionResult> DeleteProjectDetails(int projectId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            ProjectActionResult result = await _projects.DeleteProjectDetails(office, projectId, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
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

            HeadOfficeConnection? office = await _projects.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
