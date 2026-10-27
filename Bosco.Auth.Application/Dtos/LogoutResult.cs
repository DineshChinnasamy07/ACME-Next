/*  Class Name      : LogoutResult.cs
 *  Purpose         : SetLogout result for a JWT that has no revocation store
 *  Author          : Dinesh C
 *  Created on      : 26-Oct-2026
 */
namespace Bosco.Auth.Application.DTOs
{
    public class LogoutResult
    {
        public bool Success { get; set; }

        public bool TokenRevoked { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
