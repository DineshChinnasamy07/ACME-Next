/*  Class Name      : AuthServiceCollectionExtensions.cs
 *  Purpose         : Register Auth login services and the portal EF Core context
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Acme.Auth.Application.Abstractions.Repositories;
using Acme.Auth.Application.Abstractions.Services;
using Acme.Auth.Application.Features.Authentication.Login;
using Acme.Auth.Application.Features.UserManagement;
using Acme.Auth.Infrastructure.Persistence;
using Acme.Auth.Infrastructure.Repositories;
using Acme.Auth.Infrastructure.Security;
using Acme.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Auth.Infrastructure.Extensions
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
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRightRepository, RightRepository>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<UserSystem>();
            services.AddScoped<UserRoleSystem>();
            services.AddScoped<RightSystem>();
            return services;
        }
    }
}
