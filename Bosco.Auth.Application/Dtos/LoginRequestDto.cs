/*  Class Name      : LoginRequestDto.cs
 *  Purpose         : JSON login body for head office code, username, and password
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
namespace Bosco.Auth.Application.DTOs
{
    public class LoginRequestDto
    {
        public string HeadOfficeCode { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
