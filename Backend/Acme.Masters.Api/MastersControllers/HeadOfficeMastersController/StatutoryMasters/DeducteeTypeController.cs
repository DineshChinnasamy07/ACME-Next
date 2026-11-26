/*  Class Name      : DeducteeTypeController.cs
 *  Purpose         : Head Office Statutory Masters - TDS Deductee Type endpoints (legacy TDSDeducteeType.aspx)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/deducteetypes")]
    public class DeducteeTypeController : ControllerBase
    {
        private readonly DeducteeTypeSystem _deducteeTypes;

        public DeducteeTypeController(DeducteeTypeSystem deducteeTypes)
        {
            _deducteeTypes = deducteeTypes;
        }

        /// <summary>
        /// Lists every deductee type (active and inactive) with residential and deductee status captions.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchDeducteeTypeDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _deducteeTypes.FetchDeducteeTypeDetails(office, cancellationToken));
        }

        /// <summary>
        /// Returns one deductee type for editing.
        /// </summary>
        [HttpGet("{deducteeTypeId:int}")]
        public async Task<IActionResult> FillDeducteeTypeDetails(int deducteeTypeId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            DeducteeTypeListItemDto? row = await _deducteeTypes.FillDeducteeTypeDetails(office, deducteeTypeId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_TYPE_NOT_FOUND) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Inserts (DeducteeTypeId 0) or updates a deductee type.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveDeducteeTypeDetails([FromBody] DeducteeTypeSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            DeducteeTypeActionResult result = await _deducteeTypes.SaveDeducteeTypeDetails(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        /// <summary>
        /// Deletes a deductee type that no TDS policy or creditor profile uses.
        /// </summary>
        [HttpDelete("{deducteeTypeId:int}")]
        public async Task<IActionResult> DeleteDeducteeTypeDetails(int deducteeTypeId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            DeducteeTypeActionResult result = await _deducteeTypes.DeleteDeducteeTypeDetails(office, deducteeTypeId, cancellationToken);
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

            HeadOfficeConnection? office = await _deducteeTypes.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
