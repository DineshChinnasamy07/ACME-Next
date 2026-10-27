/*  Class Name      : LoginResponseDto.cs
 *  Purpose         : Successful AuthenticateUser result and access token
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
namespace Bosco.Auth.Application.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;

        public string TokenType { get; set; } = "Bearer";

        public int ExpiresInMinutes { get; set; }

        public bool PasswordChangeRequired { get; set; }

        public UserDto User { get; set; } = new UserDto();
    }
}
