/*  Class Name      : AuthenticationOutcome.cs
 *  Purpose         : AuthenticateUser result, ResultArgs meaning, and token response
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Application.DTOs;
using Bosco.Common;

namespace Bosco.Auth.Application.Features.Authentication.Login
{
    public class AuthenticationOutcome
    {
        public const string Validation = "Validation";
        public const string InvalidHeadOffice = "InvalidHeadOffice";
        public const string InvalidUser = "InvalidUser";
        public const string Unavailable = "Unavailable";

        public ResultArgs Result { get; set; } = new ResultArgs();

        public LoginResponseDto? Response { get; set; }

        public string Failure { get; set; } = string.Empty;

        public static AuthenticationOutcome Fail(string failure, string message)
        {
            ResultArgs result = new ResultArgs();
            result.Message = message;
            return new AuthenticationOutcome
            {
                Failure = failure,
                Result = result
            };
        }

        public static AuthenticationOutcome Ok(LoginResponseDto response)
        {
            ResultArgs result = new ResultArgs();
            result.Success = true;
            result.ReturnValue = response;
            return new AuthenticationOutcome
            {
                Result = result,
                Response = response
            };
        }
    }
}
