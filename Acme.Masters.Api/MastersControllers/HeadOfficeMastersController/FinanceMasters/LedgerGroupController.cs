/*  Class Name      : LedgerGroupController.cs
 *  Purpose         : Ledger Group list, lookup, get, ledgers, save, and delete endpoints (HO login)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Common;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.FinanceMasters
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/ledgergroups")]
    public class LedgerGroupController : ControllerBase
    {
        private readonly LedgerGroupSystem _groups;

        public LedgerGroupController(LedgerGroupSystem groups)
        {
            _groups = groups;
        }

        /// <summary>
        /// Lists all ledger groups of the signed-in head office (flat tree source).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLedgerGroupSource(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _groups.GetLedgerGroupSource(office, cancellationToken));
        }

        /// <summary>
        /// Parent group dropdown (excludes Bank, Cash, FD groups 12/13/14).
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> LoadLedgerGroupSource(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _groups.LoadLedgerGroupSource(office, cancellationToken));
        }

        /// <summary>
        /// Lists ledgers under the given group id list (comma-separated).
        /// </summary>
        [HttpGet("ledgers")]
        public async Task<IActionResult> GetLedgerList([FromQuery] string? groupIds, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _groups.GetLedgerList(office, groupIds, cancellationToken));
        }

        /// <summary>
        /// Loads one ledger group by GROUP_ID.
        /// </summary>
        [HttpGet("{groupId:int}")]
        public async Task<IActionResult> FillLedgerGroupDetails(int groupId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            MasterLedgerGroup? row = await _groups.FillLedgerGroupDetails(office, groupId, cancellationToken);
            if (row == null)
            {
                return NotFound(new
                {
                    message = MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_NOT_FOUND)
                });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds (GroupId = 0) or updates a ledger group.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveLedgerGroupDetails(
            [FromBody] LedgerGroupSaveRequest? request,
            CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            LedgerGroupActionResult result = await _groups.SaveLedgerGroupDetails(office, request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes a ledger group. Fixed / nature groups, children, and groups with ledgers cannot be deleted.
        /// </summary>
        [HttpDelete("{groupId:int}")]
        public async Task<IActionResult> DeleteLedgerGroup(int groupId, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            LedgerGroupActionResult result = await _groups.DeleteLedgerGroup(office, groupId, cancellationToken);
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
                    message = MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_HEAD_OFFICE_ONLY)
                }));
            }

            HeadOfficeConnection? office = await _groups.ResolveOffice(scope, cancellationToken);
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
