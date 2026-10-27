/*  Class Name      : AuthController.cs
 *  Purpose         : AuthenticateUser, SignIn, and SetLogout HTTP endpoints
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Application.DTOs;
using Bosco.Auth.Application.Features.Authentication.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserSystem _users;

        public AuthController(UserSystem users)
        {
            _users = users;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateUser([FromBody] LoginRequestDto? request, CancellationToken cancellationToken)
        {
            AuthenticationOutcome outcome = await _users.AuthenticateUser(
                request == null ? string.Empty : request.HeadOfficeCode,
                request == null ? string.Empty : request.UserName,
                request == null ? string.Empty : request.Password,
                cancellationToken);
            return Map(outcome);
        }

        [AllowAnonymous]
        [HttpPost("login/public")]
        [Consumes("application/json", "application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> SignIn([FromForm(Name = "hocode")] string? hocode, [FromForm(Name = "username")] string? username, [FromForm(Name = "password")] string? password, CancellationToken cancellationToken)
        {
            string contentType = Request.ContentType ?? string.Empty;
            if (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                LoginRequestDto? request = null;
                try
                {
                    request = await Request.ReadFromJsonAsync<LoginRequestDto>(cancellationToken);
                }
                catch (System.Text.Json.JsonException)
                {
                    request = null;
                }

                hocode = request == null ? string.Empty : request.HeadOfficeCode;
                username = request == null ? string.Empty : request.UserName;
                password = request == null ? string.Empty : request.Password;
            }

            AuthenticationOutcome outcome = await _users.AuthenticateUser(hocode ?? string.Empty, username ?? string.Empty, password ?? string.Empty, cancellationToken);
            return Map(outcome);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult SetLogout()
        {
            return Ok(_users.SetLogout());
        }

        private IActionResult Map(AuthenticationOutcome outcome)
        {
            if (outcome.Result.Success && outcome.Response != null)
            {
                return Ok(outcome.Response);
            }

            object body = new { message = outcome.Result.Message };
            if (outcome.Failure == AuthenticationOutcome.Validation || outcome.Failure == AuthenticationOutcome.InvalidHeadOffice)
            {
                return BadRequest(body);
            }

            if (outcome.Failure == AuthenticationOutcome.Unavailable)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, body);
            }

            return Unauthorized(body);
        }
    }
}
