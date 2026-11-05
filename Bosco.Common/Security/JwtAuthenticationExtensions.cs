/*  Class Name      : JwtAuthenticationExtensions.cs
 *  Purpose         : Shared JWT validation so every API accepts the Auth token
 *  Author          : Dinesh C
 *  Created on      : 05-Nov-2026
 */
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Bosco.Common.Security
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddBoscoJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = CreateValidationParameters(configuration);
                });
            services.AddAuthorization();
            return services;
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
