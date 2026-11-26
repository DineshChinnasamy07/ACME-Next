/*  Class Name      : LedgerController.cs
 *  Purpose         : Ledger list, get, group lookup, save, and delete endpoints (HO login)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Common;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Ledger;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.Ledger;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.FinanceMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/ledgers")]
    public class LedgerController : ControllerBase
    {
        private readonly LedgerSystem _ledgers;

        public LedgerController(LedgerSystem ledgers)
        {
            _ledgers = ledgers;
        }

        /// <summary>
        /// Lists HO ledgers (STATUS=0, non-branch) of the signed-in head office.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchLedgerDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _ledgers.FetchLedgerDetails(office, cancellationToken));
        }

        /// <summary>
        /// Ledger Group dropdown for Ledger add/edit (excludes root natures and Bank).
        /// </summary>
        [HttpGet("groups")]
        public async Task<IActionResult> LoadLedgerGroupforLedgerLookup(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _ledgers.LoadLedgerGroupforLedgerLookup(office, cancellationToken));
        }

        /// <summary>
        /// Loads one ledger by LEDGER_ID.
        /// </summary>
        [HttpGet("{ledgerId:int}")]
        public async Task<IActionResult> FillLedgerDetails(int ledgerId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            MasterLedger? row = await _ledgers.FillLedgerDetails(office, ledgerId, cancellationToken);
            if (row == null)
            {
                return NotFound(new
                {
                    message = MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_NOT_FOUND)
                });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds (LedgerId = 0) or updates a ledger.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveLedgerDetails(
            [FromBody] LedgerSaveRequest? request,
            CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            LedgerActionResult result = await _ledgers.SaveLedgerDetails(office, request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes a ledger. Fixed / bank / cash ledgers and ledgers with balances cannot be deleted.
        /// </summary>
        [HttpDelete("{ledgerId:int}")]
        public async Task<IActionResult> DeleteLedgerDetails(int ledgerId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            LedgerActionResult result = await _ledgers.DeleteLedgerDetails(office, ledgerId, cancellationToken);
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
                    message = MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _ledgers.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new
                {
                    message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound)
                }));
            }

            return (office, null);
        }
    }
}
