/*  Class Name      : LegalEntityController.cs
 *  Purpose         : Head Office Statutory Masters - Legal Entity list, lookups, get, save and delete endpoints (HO login)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 *  Modified on     : 24-Nov-2026 - endpoints implemented
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/legalentities")]
    public class LegalEntityController : ControllerBase
    {
        private readonly LegalEntitySystem _legalEntities;

        public LegalEntityController(LegalEntitySystem legalEntities)
        {
            _legalEntities = legalEntities;
        }

        /// <summary>
        /// Lists the legal entities (societies) of the signed-in head office.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchLegalEntity(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _legalEntities.FetchLegalEntity(office, cancellationToken));
        }

        /// <summary>
        /// Association nature and denomination options for the Legal Entity form.
        /// </summary>
        [HttpGet("lookups")]
        public async Task<IActionResult> LoadLegalEntityLookups(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(_legalEntities.LoadLegalEntityLookups());
        }

        /// <summary>
        /// Loads one legal entity by CUSTOMERID.
        /// </summary>
        [HttpGet("{customerId:int}")]
        public async Task<IActionResult> FillLegalEntityDetails(int customerId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            LegalEntityDetailDto? row = await _legalEntities.FillLegalEntityDetails(office, customerId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_NOT_FOUND) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds (CustomerId = 0) or updates a legal entity.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveLegalEntityDetails([FromBody] LegalEntitySaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            LegalEntityActionResult result = await _legalEntities.SaveLegalEntityDetails(office, request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes a legal entity. Entities linked to projects or mapped to governing bodies cannot be deleted.
        /// </summary>
        [HttpDelete("{customerId:int}")]
        public async Task<IActionResult> DeleteLegalEntityDetails(int customerId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            LegalEntityActionResult result = await _legalEntities.DeleteLegalEntityDetails(office, customerId, cancellationToken);
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

            HeadOfficeConnection? office = await _legalEntities.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
