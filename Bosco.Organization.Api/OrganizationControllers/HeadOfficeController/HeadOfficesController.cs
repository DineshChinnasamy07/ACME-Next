/*  Class Name      : HeadOfficesController.cs
 *  Purpose         : Head Office list, get, save, status, delete, active, and type endpoints
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Common;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Application.Features.Office;
using Bosco.Organization.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Organization.Api.HeadOfficeControllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "HeadOffice")]
    [ApiController]
    [Route("api/headoffices")]
    public class HeadOfficesController : ControllerBase
    {
        private readonly HeadOfficeSystem _headOffices;

        public HeadOfficesController(HeadOfficeSystem headOffices)
        {
            _headOffices = headOffices;
        }

        /// <summary>
        /// Lists all portal head offices for the grid.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The head office list.</returns>
        [HttpGet]
        public async Task<IActionResult> FetchHeadOfficeDetails(CancellationToken cancellationToken)
        {
            return Ok(await _headOffices.FetchHeadOfficeDetails(cancellationToken));
        }

        /// <summary>
        /// Lists only Activated head offices (STATUS = 2) for the Branch Office dropdown.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The activated head office list.</returns>
        [HttpGet("active")]
        public async Task<IActionResult> FetchActiveHeadOfficeDetails(CancellationToken cancellationToken)
        {
            return Ok(await _headOffices.FetchActiveHeadOfficeDetails(cancellationToken));
        }

        /// <summary>
        /// Lists head office types from head_office_type for the Type dropdown.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The head office type list.</returns>
        [HttpGet("types")]
        public async Task<IActionResult> FetchHeadOfficeType(CancellationToken cancellationToken)
        {
            return Ok(await _headOffices.FetchHeadOfficeType(cancellationToken));
        }

        /// <summary>
        /// Loads one head office by HEAD_OFFICE_ID for edit or view.
        /// </summary>
        /// <param name="headOfficeId">The head office identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The head office details when found; otherwise not found.</returns>
        [HttpGet("{headOfficeId:int}")]
        public async Task<IActionResult> HeadOfficeDetailById(int headOfficeId, CancellationToken cancellationToken)
        {
            HeadOffice? row = await _headOffices.HeadOfficeDetailById(headOfficeId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds or updates a portal head office row.
        /// </summary>
        /// <param name="request">The head office details to save.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The save result.</returns>
        [HttpPost]
        public async Task<IActionResult> SaveHeadOfficeDetails([FromBody] HeadOfficeSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeDetailsRequired) });
            }

            HeadOfficeActionResult result = await _headOffices.SaveHeadOfficeDetails(request, 0, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Lists active main branches under the head office (used to block HO deactivate).
        /// </summary>
        /// <param name="headOfficeCode">The head office code.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The active branch list for the head office.</returns>
        [HttpGet("{headOfficeCode}/branches")]
        public async Task<IActionResult> FetchBranchByHeadOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return Ok(await _headOffices.FetchBranchByHeadOffice(headOfficeCode, cancellationToken));
        }

        /// <summary>
        /// Sets the head office status to Created, Activated, or DeActivated by head office code.
        /// </summary>
        /// <param name="headOfficeCode">The head office code.</param>
        /// <param name="request">The status value to apply.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The status update result.</returns>
        [HttpPut("{headOfficeCode}/status")]
        public async Task<IActionResult> UpdateOfficeStatus(string headOfficeCode, [FromBody] OfficeStatusRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.Office.StatusRequired) });
            }

            HeadOfficeActionResult result = await _headOffices.UpdateOfficeStatus(headOfficeCode, request.Status, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates HOST_NAME, DB_NAME, USERNAME, and PASSWORD for the head office.
        /// </summary>
        /// <param name="headOfficeCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{headOfficeCode}/database")]
        public async Task<IActionResult> UpdateDatabase(string headOfficeCode, [FromBody] UpdateDatabaseRequest? request, CancellationToken cancellationToken) 
        {
            if (request == null)
            {
                return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.Office.DatabaseDetailsRequired) });
            }

            HeadOfficeActionResult result = await _headOffices.UpdateDatabase(headOfficeCode, request, cancellationToken);
            if (!result.Success) 
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);

        }

        /// <summary>
        /// Updates USER_CREATED_STATUS for the head office after admin user create / communicate.
        /// </summary>
        /// <param name="headOfficeCode">The head office code.</param>
        /// <param name="request">The user-created status value.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The update result.</returns>
        [HttpPut("{headOfficeCode}/user-created-status")]
        public async Task<IActionResult> UpdateUserCreatedStatus(string headOfficeCode, [FromBody] UpdateUserCreatedStatusRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.Office.UserCreatedStatusRequired) });
            }

            HeadOfficeActionResult result = await _headOffices.UpdateUserCreatedStatus(headOfficeCode, request.UserCreatedStatus, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes a portal head office by head office code.
        /// </summary>
        /// <param name="headOfficeCode">The head office code.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The delete result.</returns>
        [HttpDelete("{headOfficeCode}")]
        public async Task<IActionResult> DeleteHeadOfficeDetails(string headOfficeCode, CancellationToken cancellationToken)
        {
            HeadOfficeActionResult result = await _headOffices.DeleteHeadOfficeDetails(headOfficeCode, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

    }
}
