/*  Class Name      : IJwtTokenService.cs
 *  Purpose         : Access token created after AuthenticateUser succeeds
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Acme.Auth.Application.DTOs;

namespace Acme.Auth.Application.Abstractions.Services
{
    public interface IJwtTokenService
    {
        int ExpiryMinutes { get; }

        string CreateAccessToken(UserDto user);
    }
}
