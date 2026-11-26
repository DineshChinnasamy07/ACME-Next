/*  Class Name      : GoverningBodiesController.cs
 *  Purpose         : Head Office Statutory Masters - Governing Bodies list, get, save and delete endpoints (HO login)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 *  Modified on     : 24-Nov-2026 - endpoints implemented
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/governingbodies")]
    public class GoverningBodiesController : ControllerBase
    {
        private readonly ExecutiveMemberSystem _members;

        public GoverningBodiesController(ExecutiveMemberSystem members)
        {
            _members = members;
        }

        /// <summary>
        /// Lists the governing body members of the signed-in head office (ordered by name).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchExecutiveMemberDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _members.FetchExecutiveMemberDetails(office, cancellationToken));
        }

        /// <summary>
        /// Loads one governing body member by EXECUTIVE_ID.
        /// </summary>
        [HttpGet("{executiveId:int}")]
        public async Task<IActionResult> FillExecutiveMemberDetails(int executiveId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            GoverningBodyDetailDto? row = await _members.FillExecutiveMemberDetails(office, executiveId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(GoverningLegalMessages.GOVERNING_BODY_NOT_FOUND) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds (ExecutiveId = 0) or updates a governing body member.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveExecutiveMemberDetails([FromBody] GoverningBodySaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            GoverningBodyActionResult result = await _members.SaveExecutiveMemberDetails(office, request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes a governing body member. Members mapped to a legal entity cannot be deleted.
        /// </summary>
        [HttpDelete("{executiveId:int}")]
        public async Task<IActionResult> DeleteExecutiveMemberDetails(int executiveId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            GoverningBodyActionResult result = await _members.DeleteExecutiveMemberDetails(office, executiveId, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        private async Task<(HeadOfficeConnection? Office, IActionResult? Error)> ResolveOffice(CancellationToken cancellationToken)
        {
            string? scope = User.FindHeadOfficeScope();
            if (scope == null)
            {
                return (null, StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = MessageRender.GetMessage(GoverningLegalMessages.STATUTORY_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _members.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
