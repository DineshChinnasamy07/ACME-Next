/*  Class Name      : BranchOfficeController.cs
 *  Purpose         : Branch Office list, get-by-id, and save endpoints
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Application.Features.Office;
using Bosco.Organization.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Organization.Api.OrganizationControllers.BranchOfficeController
{
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
                return NotFound(new { message = "Branch Office not found." });
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
                return BadRequest(new { message = "Branch Office details are required." });
            }

            BranchOfficeActionResult result = await _branchOffices.SaveBranchOfficeDetails(request, 0, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
    }
}
