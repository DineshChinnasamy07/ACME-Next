using Acme.Common;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.FinanceMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/projectcategories")]
    public class ProjectCategoryController : ControllerBase
    {
        private readonly ProjectCatogorySystem _categories;

        public ProjectCategoryController(ProjectCatogorySystem categories)
        {
            _categories = categories;
        }

        [HttpGet]
        public async Task<IActionResult> FetchProjectCatogoryDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _categories.FetchProjectCatogoryDetails(office, cancellationToken));
        }

        [HttpGet("groups")]
        public async Task<IActionResult> FetchCategoryGroups(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _categories.FetchCategoryGroups(office, cancellationToken));
        }

        [HttpGet("itrgroups")]
        public async Task<IActionResult> FetchITRGroupCategory(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _categories.FetchITRGroupCategory(office, cancellationToken));
        }

        [HttpGet("{projectCatogoryId:int}")]
        public async Task<IActionResult> FillProjectCatogoryDetails(int projectCatogoryId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            MasterProjectCatogory? row = await _categories.FillProjectCatogoryDetails(office, projectCatogoryId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_NOT_FOUND) });
            }

            return Ok(row);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProjectCatogoryDetails([FromBody] ProjectCategorySaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            ProjectCategoryActionResult result = await _categories.SaveProjectCatogoryDetails(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpDelete("{projectCatogoryId:int}")]
        public async Task<IActionResult> DeleteProjectCatogoryDetails(int projectCatogoryId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            ProjectCategoryActionResult result = await _categories.DeleteProjectCatogoryDetails(office, projectCatogoryId, cancellationToken);
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
                    message = MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _categories.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
