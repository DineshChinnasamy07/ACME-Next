/*  Class Name      : NatureOfPaymentsController.cs
 *  Purpose         : Head Office Statutory Masters - TDS Nature of Payments endpoints (legacy TDSPaymentNature.aspx)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/natureofpayments")]
    public class NatureOfPaymentsController : ControllerBase
    {
        private readonly NatureOfPaymentSystem _natures;

        public NatureOfPaymentsController(NatureOfPaymentSystem natures)
        {
            _natures = natures;
        }

        /// <summary>
        /// Lists every nature of payment with its TDS section (active and inactive).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchNatureOfPaymentDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _natures.FetchNatureOfPaymentDetails(office, cancellationToken));
        }

        /// <summary>
        /// Active TDS sections for the Section drop-down ("CODE - SECTION_NAME").
        /// </summary>
        [HttpGet("sections")]
        public async Task<IActionResult> FetchSectionCodes(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _natures.FetchSectionCodes(office, cancellationToken));
        }

        /// <summary>
        /// Returns one nature of payment for editing.
        /// </summary>
        [HttpGet("{naturePayId:int}")]
        public async Task<IActionResult> FillNatureOfPaymentDetails(int naturePayId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            NatureOfPaymentListItemDto? row = await _natures.FillNatureOfPaymentDetails(office, naturePayId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_NOT_FOUND) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Inserts (NaturePayId 0) or updates a nature of payment.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveNatureOfPaymentDetails([FromBody] NatureOfPaymentSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            NatureOfPaymentActionResult result = await _natures.SaveNatureOfPaymentDetails(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        /// <summary>
        /// Deletes a nature of payment that no TDS policy or creditor profile uses.
        /// </summary>
        [HttpDelete("{naturePayId:int}")]
        public async Task<IActionResult> DeleteNatureOfPaymentDetails(int naturePayId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            NatureOfPaymentActionResult result = await _natures.DeleteNatureOfPaymentDetails(office, naturePayId, cancellationToken);
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

            HeadOfficeConnection? office = await _natures.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
