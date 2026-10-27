/*  Class Name      : JwtTokenService.cs
 *  Purpose         : Sign the access token after AuthenticateUser
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bosco.Auth.Application.Abstractions.Services;
using Bosco.Auth.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Bosco.Auth.Infrastructure.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        public const string PasswordStatusClaim = "PASSWORD_STATUS";
        public const string HeadOfficeCodeClaim = "HEAD_OFFICE_CODE";
        public const string PasswordChangeRequiredClaim = "password_change_required";

        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _key;

        public JwtTokenService(IConfiguration configuration)
        {
            _issuer = configuration["Jwt:Issuer"] ?? string.Empty;
            _audience = configuration["Jwt:Audience"] ?? string.Empty;
            _key = configuration["Jwt:Key"] ?? string.Empty;
            string expiry = configuration["Jwt:ExpiryMinutes"] ?? string.Empty;
            int minutes;
            ExpiryMinutes = int.TryParse(expiry, out minutes) && minutes > 0 ? minutes : 60;
            if (string.IsNullOrWhiteSpace(_issuer) || string.IsNullOrWhiteSpace(_audience) || string.IsNullOrWhiteSpace(_key))
            {
                throw new InvalidOperationException("Jwt:Issuer, Jwt:Audience, and Jwt:Key are required.");
            }
        }

        public int ExpiryMinutes { get; private set; }

        public string CreateAccessToken(UserDto user)
        {
            SigningCredentials credentials = new SigningCredentials(SigningKey(_key), SecurityAlgorithms.HmacSha256);
            Claim[] claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, user.UserRole ?? string.Empty),
                new Claim(PasswordStatusClaim, user.PasswordStatus.ToString(CultureInfo.InvariantCulture)),
                new Claim(HeadOfficeCodeClaim, user.HeadOfficeCode ?? string.Empty),
                new Claim(PasswordChangeRequiredClaim, user.PasswordChangeRequired ? "true" : "false")
            };
            JwtSecurityToken token = new JwtSecurityToken(_issuer, _audience, claims, expires: DateTime.UtcNow.AddMinutes(ExpiryMinutes), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static TokenValidationParameters CreateValidationParameters(IConfiguration configuration)
        {
            string issuer = configuration["Jwt:Issuer"] ?? string.Empty;
            string audience = configuration["Jwt:Audience"] ?? string.Empty;
            string key = configuration["Jwt:Key"] ?? string.Empty;
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = SigningKey(key),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        }

        private static SymmetricSecurityKey SigningKey(string key)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }
    }
}
