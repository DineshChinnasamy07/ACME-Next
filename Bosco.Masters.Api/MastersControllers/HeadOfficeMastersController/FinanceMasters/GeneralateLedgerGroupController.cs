/*  Class Name      : GeneralateLedgerGroupController.cs
 *  Purpose         : Finance Masters - Generalate Ledger Group endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common;
using Bosco.Common.Security;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Masters.Api.MastersControllers.HeadOfficeMastersController.FinanceMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/generalateledgergroups")]
    public class GeneralateLedgerGroupController : ControllerBase
    {
        private readonly GeneralateSystem _ledgers;

        public GeneralateLedgerGroupController(GeneralateSystem ledgers)
        {
            _ledgers = ledgers;
        }

        [HttpGet]
        public async Task<IActionResult> FetchAllGeneralateLedgers(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _ledgers.FetchAllGeneralateLedgers(office, cancellationToken));
        }

        [HttpGet("parents")]
        public async Task<IActionResult> FetchAllGeneralateParentLedgers(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _ledgers.FetchAllGeneralateParentLedgers(office, cancellationToken));
        }

        [HttpGet("{conLedgerId:int}")]
        public async Task<IActionResult> FetchGeneralateLedgerById(int conLedgerId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            CongregationLedger? row = await _ledgers.FetchGeneralateLedgerById(office, conLedgerId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_NOT_FOUND) });
            }

            return Ok(row);
        }

        [HttpPost]
        public async Task<IActionResult> SaveGeneralateLedger([FromBody] GeneralateLedgerSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            GeneralateLedgerActionResult result = await _ledgers.SaveGeneralateLedger(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpDelete("{conLedgerId:int}")]
        public async Task<IActionResult> DeleteLedger(int conLedgerId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            GeneralateLedgerActionResult result = await _ledgers.DeleteLedger(office, conLedgerId, cancellationToken);
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
                    message = MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _ledgers.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
