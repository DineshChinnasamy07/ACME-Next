/*  Class Name      : AuthServiceCollectionExtensions.cs
 *  Purpose         : Register Auth login services and the portal EF Core context
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Application.Abstractions.Repositories;
using Bosco.Auth.Application.Abstractions.Services;
using Bosco.Auth.Application.Features.Authentication.Login;
using Bosco.Auth.Infrastructure.Persistence;
using Bosco.Auth.Infrastructure.Repositories;
using Bosco.Auth.Infrastructure.Security;
using Bosco.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Auth.Infrastructure.Extensions
{
    public static class AuthServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new InvalidOperationException("Auth configuration is missing.");
            }

            string connectionString = configuration.GetConnectionString(DatabaseServiceCollectionExtensions.ConnectionName) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'AcpErp' is missing. Set ConnectionStrings:AcpErp in appsettings.json.");
            }

            services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IHeadOfficeRepository, HeadOfficeRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<UserSystem>();
            return services;
        }
    }
}
