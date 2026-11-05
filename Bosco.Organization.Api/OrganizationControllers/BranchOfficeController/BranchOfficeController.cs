/*  Class Name      : BranchOfficeController.cs
 *  Purpose         : Branch Office list, get-by-id, save, status, and delete endpoints
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using Bosco.Common;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Application.Features.Office;
using Bosco.Organization.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Organization.Api.OrganizationControllers.BranchOfficeController
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "BranchOffice")]
    [ApiController]
    [Route("api/branchoffices")]
    public class BranchOfficeController : ControllerBase
    {
        private readonly BranchOfficeSystem _branchOffices;

        public BranchOfficeController(BranchOfficeSystem branchOffices)
        {
            _branchOffices = branchOffices;
        }

        /// <summary>
        /// Lists all portal branch offices for the grid.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The branch office list.</returns>
        [HttpGet]
        public async Task<IActionResult> FetchBranchOfficeDetails(CancellationToken cancellationToken)
        {
            return Ok(await _branchOffices.FetchBranchOfficeDetails(cancellationToken));
        }

        /// <summary>
        /// Loads one branch office by BRANCH_OFFICE_ID for edit or view.
        /// </summary>
        /// <param name="branchOfficeId">The branch office identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The branch office details when found; otherwise not found.</returns>
        [HttpGet("{branchOfficeId:int}")]
        public async Task<IActionResult> FillBranchOfficeDetails(int branchOfficeId, CancellationToken cancellationToken)
        {
            BranchOffice? row = await _branchOffices.FillBranchOfficeDetails(branchOfficeId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeNotFound) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds or updates a portal branch office row.
        /// </summary>
        /// <param name="request">The branch office details to save.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The save result.</returns>
        [HttpPost]
        public async Task<IActionResult> SaveBranchOfficeDetails([FromBody] BranchOfficeSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDetailsRequired) });
            }

            BranchOfficeActionResult result = await _branchOffices.SaveBranchOfficeDetails(request, 0, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Sets the branch office status to Created, Activated, or DeActivated by branch office code.
        /// </summary>
        /// <param name="branchOfficeCode">The full branch office code.</param>
        /// <param name="request">The status value to apply.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The status update result.</returns>
        [HttpPut("{branchOfficeCode}/status")]
        public async Task<IActionResult> UpdateOfficeStatus(string branchOfficeCode, [FromBody] OfficeStatusRequest? request, CancellationToken cancellationToken) 
        {
            if (request == null)
            {
              return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.Office.StatusRequired) });
            }
            BranchOfficeActionResult result = await _branchOffices.UpdateOfficeStatus(branchOfficeCode, request.Status, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);
        }

        /// <summary>
        /// Deletes a portal branch office by branch office code.
        /// </summary>
        /// <param name="branchOfficeCode">The full branch office code.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The delete result.</returns>
        [HttpDelete("{branchOfficeCode}")]
        public async Task<IActionResult> DeleteBranchOfficeDetails(string branchOfficeCode, CancellationToken cancellationToken)
        {
            BranchOfficeActionResult result = await _branchOffices.DeleteBranchOfficeDetails(branchOfficeCode, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);
        }
    }
}
