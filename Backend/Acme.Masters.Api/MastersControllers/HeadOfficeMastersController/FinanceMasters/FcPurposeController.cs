/*  Class Name      : FcPurposeController.cs
 *  Purpose         : Finance Masters - FC Purpose endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Common;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.FinanceMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/fcpurposes")]
    public class FcPurposeController : ControllerBase
    {
        private readonly PurposeSystem _purposes;

        public FcPurposeController(PurposeSystem purposes)
        {
            _purposes = purposes;
        }

        [HttpGet]
        public async Task<IActionResult> FetchPurposeDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _purposes.FetchPurposeDetails(office, cancellationToken));
        }

        [HttpGet("{contributionId:int}")]
        public async Task<IActionResult> FillPurposeDetails(int contributionId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            MasterContributionHead? row = await _purposes.FillPurposeDetails(office, contributionId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_NOT_FOUND) });
            }

            return Ok(row);
        }

        [HttpPost]
        public async Task<IActionResult> SavePurposeDetails([FromBody] FcPurposeSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            FcPurposeActionResult result = await _purposes.SavePurposeDetails(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpDelete("{contributionId:int}")]
        public async Task<IActionResult> DeletePurposeDetails(int contributionId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            FcPurposeActionResult result = await _purposes.DeletePurposeDetails(office, contributionId, cancellationToken);
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
                    message = MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _purposes.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
