/*  Class Name      : TdsPolicyController.cs
 *  Purpose         : Head Office Statutory Masters - TDS Policy endpoints (TDS_POLICY + TDS_TAX_RATE)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 *  Modified on     : 24-Nov-2026 - list / lookups / get / save / delete
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/tdspolicies")]
    public class TdsPolicyController : ControllerBase
    {
        private readonly TdsPolicySystem _policies;

        public TdsPolicyController(TdsPolicySystem policies)
        {
            _policies = policies;
        }

        /// <summary>
        /// Lists every TDS policy of the head office with deductee type, nature of payment, section names
        /// and the tax rate rows (rate % / exemption limit per duty / tax type).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchTdsPolicyDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _policies.FetchTdsPolicyDetails(office, cancellationToken));
        }

        /// <summary>
        /// Form lookups: active deductee types, active nature of payments and active duty / tax types.
        /// </summary>
        [HttpGet("lookups")]
        public async Task<IActionResult> FetchTdsPolicyLookups(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _policies.FetchTdsPolicyLookups(office, cancellationToken));
        }

        /// <summary>
        /// One TDS policy with its tax rate rows.
        /// </summary>
        [HttpGet("{tdsPolicyId:int}")]
        public async Task<IActionResult> FillTdsPolicyDetails(int tdsPolicyId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            TdsPolicyDetailDto? policy = await _policies.FillTdsPolicyDetails(office, tdsPolicyId, cancellationToken);
            if (policy == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_NOT_FOUND) });
            }

            return Ok(policy);
        }

        /// <summary>
        /// Saves a TDS policy and its tax rate rows in one transaction (TdsPolicyId 0 = new; on edit the
        /// tax rate rows are replaced).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveTdsPolicyDetails([FromBody] TdsPolicySaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            TdsPolicyActionResult result = await _policies.SaveTdsPolicyDetails(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        /// <summary>
        /// Deletes a TDS policy together with its tax rate rows.
        /// </summary>
        [HttpDelete("{tdsPolicyId:int}")]
        public async Task<IActionResult> DeleteTdsPolicyDetails(int tdsPolicyId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            TdsPolicyActionResult result = await _policies.DeleteTdsPolicyDetails(office, tdsPolicyId, cancellationToken);
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
                    message = MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _policies.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
