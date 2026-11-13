/*  Class Name      : JwtAuthenticationExtensions.cs
 *  Purpose         : Shared JWT validation so every API accepts the Auth token
 *  Author          : Dinesh C
 *  Created on      : 05-Nov-2026
 */
#nullable enable
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Bosco.Common.Security
{
    public static class JwtAuthenticationExtensions
    {
        // Portal admin tokens carry an empty HEAD_OFFICE_CODE. Head office / branch tokens carry their code.
        public const string PortalOnlyPolicy = "PortalOnly";
        public const string HeadOfficeCodeClaim = "HEAD_OFFICE_CODE";

        // Portal admin or Head Office user. Branch users (BRANCH_CODE set) are rejected.
        public const string PortalOrHeadOfficePolicy = "PortalOrHeadOffice";
        public const string BranchCodeClaim = "BRANCH_CODE";

        public static IServiceCollection AddBoscoJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = CreateValidationParameters(configuration);
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PortalOnlyPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                    {
                        string? code = context.User.FindFirst(HeadOfficeCodeClaim)?.Value;
                        return string.IsNullOrWhiteSpace(code);
                    });
                });
                options.AddPolicy(PortalOrHeadOfficePolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                    {
                        string? branchCode = context.User.FindFirst(BranchCodeClaim)?.Value;
                        return string.IsNullOrWhiteSpace(branchCode);
                    });
                });
            });
            return services;
        }

        // Returns null for a portal user (no limit). Returns the lowercase head office code for a head office user.
        public static string? FindHeadOfficeScope(this ClaimsPrincipal user)
        {
            string? code = user.FindFirst(HeadOfficeCodeClaim)?.Value;
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            return code.Trim().ToLowerInvariant();
        }

        public static TokenValidationParameters CreateValidationParameters(IConfiguration configuration)
        {
            string issuer = configuration["Jwt:Issuer"] ?? string.Empty;
            string audience = configuration["Jwt:Audience"] ?? string.Empty;
            string key = configuration["Jwt:Key"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("Jwt:Issuer, Jwt:Audience, and Jwt:Key are required.");
            }

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        }
    }
}
