/*  Class Name      : AccountingPeriodController.cs
 *  Purpose         : Accounting Period list, get, save, delete, and status endpoints (HO login)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common;
using Bosco.Common.Security;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Masters.Api.MastersControllers.HeadOfficeMastersController.FinanceMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/accountingperiods")]
    public class AccountingPeriodController : ControllerBase
    {
        private readonly AccountingPeriodSystem _periods;

        public AccountingPeriodController(AccountingPeriodSystem periods)
        {
            _periods = periods;
        }

        /// <summary>
        /// Lists accounting periods of the signed-in head office.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FetchAccountingPeriodDetails(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _periods.FetchAccountingPeriodDetails(office, cancellationToken));
        }

        /// <summary>
        /// Loads one accounting period by ACC_YEAR_ID.
        /// </summary>
        [HttpGet("{accPeriodId:int}")]
        public async Task<IActionResult> FillAccountingPeriodDetails(int accPeriodId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            AccountingYear? row = await _periods.FillAccountingPeriodDetails(office, accPeriodId, cancellationToken);
            if (row == null)
            {
                return NotFound(new
                {
                    message = MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_NOT_FOUND)
                });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds (AccPeriodId = 0) or updates an accounting period.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveAccountingPeriodDetails(
            [FromBody] AccountingPeriodSaveRequest? request,
            CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            AccountingPeriodActionResult result = await _periods.SaveAccountingPeriodDetails(office, request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes an accounting period. Active periods and periods with ledger balances cannot be deleted.
        /// </summary>
        [HttpDelete("{accPeriodId:int}")]
        public async Task<IActionResult> DeleteAccountingPeriodDetails(int accPeriodId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            AccountingPeriodActionResult result = await _periods.DeleteAccountingPeriodDetails(office, accPeriodId, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Activates an accounting period (STATUS=1) and deactivates all others.
        /// </summary>
        [HttpPut("{accPeriodId:int}/status")]
        public async Task<IActionResult> UpdateOfficeStatus(
            int accPeriodId,
            [FromBody] AccountingPeriodStatusRequest? request,
            CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            int status = request == null ? 1 : request.Status;
            AccountingPeriodActionResult result = await _periods.UpdateOfficeStatus(office, accPeriodId, status, cancellationToken);
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
                    message = MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _periods.ResolveOffice(scope, cancellationToken);
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
