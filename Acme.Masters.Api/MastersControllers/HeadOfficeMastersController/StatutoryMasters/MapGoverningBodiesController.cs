/*  Class Name      : MapGoverningBodiesController.cs
 *  Purpose         : Head Office Statutory Masters - Map Governing Bodies to a Legal Entity (HO login)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 *  Modified on     : 24-Nov-2026 - endpoints implemented
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapgoverningbodies")]
    public class MapGoverningBodiesController : ControllerBase
    {
        private readonly GoverningMemberMappingSystem _mappings;

        public MapGoverningBodiesController(GoverningMemberMappingSystem mappings)
        {
            _mappings = mappings;
        }

        /// <summary>
        /// Legal Entity dropdown (id = CUSTOMERID, name = society name).
        /// </summary>
        [HttpGet("legalentities")]
        public async Task<IActionResult> LoadLegalEntityLookup(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _mappings.LoadLegalEntityLookup(office, cancellationToken));
        }

        /// <summary>
        /// All governing body members with Selected = mapped to the legal entity (mapped rows first).
        /// </summary>
        [HttpGet("{customerId:int}")]
        public async Task<IActionResult> FetchGoverningMembersByLegalEntity(int customerId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            IReadOnlyList<GoverningBodyMappingRowDto>? rows = await _mappings.FetchGoverningMembersByLegalEntity(office, customerId, cancellationToken);
            if (rows == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_NOT_FOUND) });
            }

            return Ok(rows);
        }

        /// <summary>
        /// Replaces the legal entity's mapping with the ticked governing body members (empty list clears it).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MapGoverningMembers([FromBody] GoverningBodyMappingSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            GoverningBodyMappingActionResult result = await _mappings.MapGoverningMembers(office, request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Removes every governing body mapping of the legal entity.
        /// </summary>
        [HttpDelete("{customerId:int}")]
        public async Task<IActionResult> DeleteMappedGoverningMember(int customerId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            GoverningBodyMappingActionResult result = await _mappings.DeleteMappedGoverningMember(office, customerId, cancellationToken);
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

            HeadOfficeConnection? office = await _mappings.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
