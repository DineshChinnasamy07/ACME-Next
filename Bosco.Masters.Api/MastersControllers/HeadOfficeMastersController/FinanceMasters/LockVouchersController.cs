/*  Class Name      : LockVouchersController.cs
 *  Purpose         : Finance Masters - Lock Voucher endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common;
using Bosco.Common.Security;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Masters.Api.MastersControllers.HeadOfficeMastersController.FinanceMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/lockvouchers")]
    public class LockVouchersController : ControllerBase
    {
        private readonly LockVoucherSystem _locks;

        public LockVouchersController(LockVoucherSystem locks)
        {
            _locks = locks;
        }

        [HttpGet]
        public async Task<IActionResult> FetchLockedProjects(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _locks.FetchLockedProjects(office, cancellationToken));
        }

        [HttpGet("branches")]
        public async Task<IActionResult> FetchBranches(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _locks.FetchBranches(office, cancellationToken));
        }

        [HttpGet("projects")]
        public async Task<IActionResult> FetchProjectsForVoucherLock([FromQuery] int branchId, [FromQuery] int lockTransId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _locks.FetchProjectsForVoucherLock(office, branchId, lockTransId, cancellationToken));
        }

        [HttpGet("{lockTransId:int}")]
        public async Task<IActionResult> FillLockVoucherDetails(int lockTransId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            MasterLockTrans? row = await _locks.FillLockVoucherDetails(office, lockTransId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_NOT_FOUND) });
            }

            return Ok(new
            {
                row.LockTransId,
                row.BranchId,
                row.ProjectId,
                row.DateFrom,
                row.DateTo,
                row.Reason,
                row.LockType,
                row.LockByPortal
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveLockVoucher([FromBody] LockVoucherSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            LockVoucherActionResult result = await _locks.SaveLockVoucher(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpDelete("{lockTransId:int}")]
        public async Task<IActionResult> DeleteLockDetails(int lockTransId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            LockVoucherActionResult result = await _locks.DeleteLockDetails(office, lockTransId, cancellationToken);
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
                    message = MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _locks.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
