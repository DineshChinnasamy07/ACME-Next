/*  Class Name      : TdsSectionController.cs
 *  Purpose         : Head Office Statutory Masters - TDS Section endpoints (legacy TDSSectionView.aspx)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/tdssections")]
    public class TdsSectionController : ControllerBase
    {
        private readonly TdsSectionSystem _sections;

        public TdsSectionController(TdsSectionSystem sections)
        {
            _sections = sections;
        }

        /// <summary>
        /// Lists every TDS section of the Head Office (active and inactive), ordered by code.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchTdsSectionDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _sections.FetchTdsSectionDetails(office, cancellationToken));
        }

        /// <summary>
        /// Returns one TDS section for editing.
        /// </summary>
        [HttpGet("{tdsSectionId:int}")]
        public async Task<IActionResult> FillTdsSectionDetails(int tdsSectionId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            TdsSectionListItemDto? row = await _sections.FillTdsSectionDetails(office, tdsSectionId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_NOT_FOUND) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Inserts (TdsSectionId 0) or updates a TDS section.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveTdsSectionDetails([FromBody] TdsSectionSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            TdsSectionActionResult result = await _sections.SaveTdsSectionDetails(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        /// <summary>
        /// Deletes a TDS section that no nature of payment uses.
        /// </summary>
        [HttpDelete("{tdsSectionId:int}")]
        public async Task<IActionResult> DeleteTdsSectionDetails(int tdsSectionId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            TdsSectionActionResult result = await _sections.DeleteTdsSectionDetails(office, tdsSectionId, cancellationToken);
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
                    message = MessageRender.GetMessage(TdsMasterMessages.TDS_MASTERS_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _sections.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
