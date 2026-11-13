/*  Class Name      : BranchLocationController.cs
 *  Purpose         : Branch Location list, get, branches, save, and delete endpoints (Head Office login)
 *  Author          : Dinesh C
 *  Created on      : 13-Nov-2026
 */
using Bosco.Common;
using Bosco.Common.Security;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Application.Features.Office;
using Bosco.Organization.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Organization.Api.OrganizationControllers.BranchLocationController
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "BranchLocation")]
    [ApiController]
    [Route("api/branchlocations")]
    public class BranchLocationController : ControllerBase
    {
        private readonly BranchLocationSystem _locations;

        public BranchLocationController(BranchLocationSystem locations)
        {
            _locations = locations;
        }

        /// <summary>
        /// Lists the branch locations of the signed-in head office.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The branch location list.</returns>
        [HttpGet]
        public async Task<IActionResult> FetchBranchLocationAll(CancellationToken cancellationToken)
        {
            (HeadOffice? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _locations.FetchBranchLocationAll(office, cancellationToken));
        }

        /// <summary>
        /// Lists the activated main branches a location can belong to.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The branch list for the dropdown.</returns>
        [HttpGet("branches")]
        public async Task<IActionResult> FetchBranch(CancellationToken cancellationToken)
        {
            (HeadOffice? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            return Ok(await _locations.FetchBranch(office, cancellationToken));
        }

        /// <summary>
        /// Loads one branch location by LOCATION_ID.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The location when found; otherwise not found.</returns>
        [HttpGet("{locationId:int}")]
        public async Task<IActionResult> FillBranchLocationDetails(int locationId, CancellationToken cancellationToken)
        {
            (HeadOffice? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            BranchLocationListItemDto? row = await _locations.FillBranchLocationDetails(office, locationId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.BranchLocationNotFound) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds (LocationId = 0) or updates a branch location.
        /// </summary>
        /// <param name="request">The location details to save.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The save result.</returns>
        [HttpPost]
        public async Task<IActionResult> SaveBranchLocationDetails([FromBody] BranchLocationSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOffice? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            BranchLocationActionResult result = await _locations.SaveBranchLocationDetails(office, request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes a branch location. The Primary location and locations used by project mapping cannot be deleted.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The delete result.</returns>
        [HttpDelete("{locationId:int}")]
        public async Task<IActionResult> DeleteLocation(int locationId, CancellationToken cancellationToken)
        {
            (HeadOffice? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null)
            {
                return error!;
            }

            BranchLocationActionResult result = await _locations.DeleteLocation(office, locationId, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        // Branch locations live in the head office database, so a portal login has nothing to read.
        private async Task<(HeadOffice? Office, IActionResult? Error)> ResolveOffice(CancellationToken cancellationToken)
        {
            string? scope = User.FindHeadOfficeScope();
            if (scope == null)
            {
                return (null, StatusCode(StatusCodes.Status403Forbidden, new { message = MessageRender.GetMessage(MessageCatalog.Office.BranchLocationHeadOfficeOnly) }));
            }

            HeadOffice? office = await _locations.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
