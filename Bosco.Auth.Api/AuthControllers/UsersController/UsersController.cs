/*  Class Name      : UsersController.cs
 *  Purpose         : User list, get, save, delete, and status endpoints
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */

using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Application.DTOs;
using Bosco.Auth.Application.Features.Authentication.Login;
using Bosco.Auth.Infrastructure.Security;
using Bosco.Common.Security;
using MessageCatalog = Bosco.Common.MessageCatalog;
using MessageRender = Bosco.Common.MessageRender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bosco.Auth.Api.Controllers
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "Users")]
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserSystem _users;

        public UsersController(UserSystem users)
        {
            _users = users;
        }

        #region Propertys
        public class UserStatusRequest 
        {
            public int Status { get; set; }
        }


        #endregion

        #region Methodes
        private int ReadUserId() 
        {
            string? value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId;
            if (!int.TryParse(value, out userId)) 
            {
                return 0;
            }

            return userId;
        }

        private string ReadHeadOfficeCode()
        {
            return User.FindFirstValue(JwtTokenService.HeadOfficeCodeClaim) ?? string.Empty;
        }

        #endregion


        #region Controller 

        /// <summary>
        /// Fetches the user details for the signed-in creator.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The user details.</returns>
        [HttpGet]
        public async Task<IActionResult> FetchUserDetail(CancellationToken cancellationToken)
        {
            int createdBy = ReadUserId();
            if (createdBy <= 0)
            {
                return Unauthorized();
            }
            return Ok(await _users.FetchUserDetail(createdBy, ReadHeadOfficeCode(), cancellationToken));
        }

        /// <summary>
        /// Lists the roles for the user form dropdown (portal roles, or the head office database roles).
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The role list.</returns>
        [HttpGet("roles")]
        public async Task<IActionResult> FetchUserRoles(CancellationToken cancellationToken)
        {
            return Ok(await _users.FetchUserRoles(ReadHeadOfficeCode(), cancellationToken));
        }

        /// <summary>
        /// Fetches one user by user id for edit or view.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The user details when found; otherwise not found.</returns>
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> FetchUserDetail(int userId, CancellationToken cancellationToken)
        {
            UserSaveRequest? row = await _users.FetchUserDetailsById(userId, ReadHeadOfficeCode(), cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = MessageRender.GetMessage(MessageCatalog.User.NotFound) });
            }
            return Ok(row);
        }

        /// <summary>
        /// Adds or updates a user.
        /// </summary>
        /// <param name="request">The user details to save.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The save result.</returns>
        [HttpPost]
        public async Task<IActionResult> SaveUser([FromBody] UserSaveRequest? request, CancellationToken cancellationToken)
        {
            int createdBy = ReadUserId();
            if (createdBy <= 0)
            {
                return Unauthorized();
            }
            if (request == null)
            {
                return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.User.FieldsRequired) });
            }

            UserActionResult result = await _users.SaveUser(request, createdBy, ReadHeadOfficeCode(), cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);
        }

        /// <summary>
        /// Deletes a user by user id.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The delete result.</returns>
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
        {
            UserActionResult result = await _users.DeleteUser(userId, ReadHeadOfficeCode(), cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);
        }

        /// <summary>
        /// Updates the active or inactive status of a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="request">The status value to apply.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The status update result.</returns>
        [HttpPut("{userId:int}/status")]
        public async Task<IActionResult> UpdateUserStatus(int userId, [FromBody] UserStatusRequest? request, CancellationToken cancellationToken) 
        {
            if (request == null) 
            {
                return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.User.StatusInvalid) });
            }
            UserActionResult result = await _users.UpdateUserStatus(userId, request.Status, ReadHeadOfficeCode(), cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);
        }

        /// <summary>
        /// Sets a new password for the selected user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="request">The old password, new password, and confirm password.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The password change result.</returns>
        [HttpPost("{userId:int}/password")]
        public async Task<IActionResult> SetPassword(int userId, [FromBody] SetPasswordRequest? request, CancellationToken cancellationToken)
        {
            int createdBy = ReadUserId();
            if (createdBy <= 0)
            {
                return Unauthorized();
            }

            if (request == null)
            {
                return BadRequest(new { message = "Old password, new password, and confirm password are required." });
            }

            UserActionResult result = await _users.SetPassword(userId, ReadHeadOfficeCode(), request.OldPassword, request.NewPassword, request.ConfirmPassword, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Sets a new password for a user without the old password (admin reset). The user must change it at the next login.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="request">The new password and confirm password.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The password reset result.</returns>
        [HttpPost("{userId:int}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(int userId, [FromBody] ResetPasswordRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { message = MessageRender.GetMessage(MessageCatalog.User.PasswordMismatch) });
            }

            UserActionResult result = await _users.ResetUserPassword(userId, ReadHeadOfficeCode(), request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

    }
        #endregion

}
