/*  Class Name      : UsersController.cs
 *  Purpose         : Auth service file
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */

using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Application.DTOs;
using Bosco.Auth.Application.Features.Authentication.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bosco.Auth.Api.Controllers
{
    [Authorize]
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

        #endregion


        #region Controller 

        [HttpGet]
        public async Task<IActionResult> FetchUserDetail(CancellationToken cancellationToken)
        {
            int createdBy = ReadUserId();
            if (createdBy <= 0)
            {
                return Unauthorized();
            }
            return Ok(await _users.FetchUserDetail(createdBy, cancellationToken));
        }
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> FetchUserDetail(int userId, CancellationToken cancellationToken)
        {
            UserSaveRequest? row = await _users.FetchUserDetailsById(userId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(row);
        }

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
                return BadRequest(new { message = "User details are required." });
            }

            UserActionResult result = await _users.SaveUser(request, createdBy, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);
        }

        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
        {
            UserActionResult result = await _users.DeleteUser(userId, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);
        }
        [HttpPut("{userId:int}/status")]
        public async Task<IActionResult> UpdateUserStatus(int userId, [FromBody] UserStatusRequest? request, CancellationToken cancellationToken) 
        {
            if (request == null) 
            {
                return BadRequest(new { message = "status is required" });
            }
            UserActionResult result = await _users.UpdateUserStatus(userId, request.Status, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result);
        }

    }
        #endregion

}
