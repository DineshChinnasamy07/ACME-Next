/*  Class Name      : DutyTaxController.cs
 *  Purpose         : Head Office Statutory Masters - TDS Duty / Tax type endpoints (legacy TDSDutyTax.aspx)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/dutytaxes")]
    public class DutyTaxController : ControllerBase
    {
        private readonly DutyTaxSystem _dutyTaxes;

        public DutyTaxController(DutyTaxSystem dutyTaxes)
        {
            _dutyTaxes = dutyTaxes;
        }

        /// <summary>
        /// Lists every duty / tax type (active and inactive); IsSystem marks the built-in types 1-5.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchDutyTaxDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _dutyTaxes.FetchDutyTaxDetails(office, cancellationToken));
        }

        /// <summary>
        /// Returns one duty / tax type for editing.
        /// </summary>
        [HttpGet("{tdsDutyTaxTypeId:int}")]
        public async Task<IActionResult> FillDutyTaxDetails(int tdsDutyTaxTypeId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            DutyTaxListItemDto? row = await _dutyTaxes.FillDutyTaxDetails(office, tdsDutyTaxTypeId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_NOT_FOUND) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Inserts (TdsDutyTaxTypeId 0) or updates a duty / tax type. Built-in types accept a status change only.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveDutyTaxDetails([FromBody] DutyTaxSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            DutyTaxActionResult result = await _dutyTaxes.SaveDutyTaxDetails(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        /// <summary>
        /// Deletes a user-defined duty / tax type that no TDS tax rate uses.
        /// </summary>
        [HttpDelete("{tdsDutyTaxTypeId:int}")]
        public async Task<IActionResult> DeleteDutyTaxDetails(int tdsDutyTaxTypeId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            DutyTaxActionResult result = await _dutyTaxes.DeleteDutyTaxDetails(office, tdsDutyTaxTypeId, cancellationToken);
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

            HeadOfficeConnection? office = await _dutyTaxes.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
