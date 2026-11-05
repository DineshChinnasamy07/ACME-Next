/*  Class Name      : SwaggerBasicAuthExtensions.cs
 *  Purpose         : Require Basic Auth before Swagger UI or swagger.json can be opened
 *  Author          : Dinesh C
 *  Created on      : 05-Nov-2026
 */
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Common.Swagger
{
    public static class SwaggerBasicAuthExtensions
    {
        public static IApplicationBuilder UseBoscoSwaggerBasicAuth(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                if (!IsSwaggerPath(context.Request.Path))
                {
                    await next();
                    return;
                }

                IConfiguration configuration = context.RequestServices.GetRequiredService<IConfiguration>();
                string expectedUser = configuration["Swagger:Username"] ?? string.Empty;
                string expectedPassword = configuration["Swagger:Password"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(expectedUser) || string.IsNullOrWhiteSpace(expectedPassword))
                {
                    await Challenge(context);
                    return;
                }

                string user;
                string password;
                if (!TryReadBasicCredentials(context.Request, out user, out password)
                    || !FixedEquals(user, expectedUser)
                    || !FixedEquals(password, expectedPassword))
                {
                    await Challenge(context);
                    return;
                }

                await next();
            });
        }

        private static bool IsSwaggerPath(PathString path)
        {
            string value = path.Value ?? string.Empty;
            return value.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryReadBasicCredentials(HttpRequest request, out string user, out string password)
        {
            user = string.Empty;
            password = string.Empty;
            string header = request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                string encoded = header.Substring("Basic ".Length).Trim();
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                int colon = decoded.IndexOf(':');
                if (colon < 0)
                {
                    return false;
                }

                user = decoded.Substring(0, colon);
                password = decoded.Substring(colon + 1);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool FixedEquals(string left, string right)
        {
            byte[] leftBytes = Encoding.UTF8.GetBytes(left ?? string.Empty);
            byte[] rightBytes = Encoding.UTF8.GetBytes(right ?? string.Empty);
            if (leftBytes.Length != rightBytes.Length)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

        private static Task Challenge(HttpContext context)
        {
            context.Response.Headers.WWWAuthenticate = "Basic realm=\"Swagger\"";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
    }
}
