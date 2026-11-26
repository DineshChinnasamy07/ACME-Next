/*  Class Name      : OrganizationServiceCollectionExtensions.cs
 *  Purpose         : Register Organization EF DbContext and Head Office services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Acme.Organization.Application.Abstractions.Repositories;
using Acme.Organization.Application.Abstractions.Services;
using Acme.Organization.Application.Features.Office;
using Acme.Organization.Infrastructure.Persistence;
using Acme.Organization.Infrastructure.Repositories;
using Acme.Organization.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Organization.Infrastructure.Extensions
{
    public static class OrganizationServiceCollectionExtensions
    {
        public const string ConnectionName = "AcpErp";

        public static IServiceCollection AddOrganizationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString(ConnectionName) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'AcpErp' is missing. Set ConnectionStrings:AcpErp in appsettings.json.");
            }

            services.AddDbContext<OrganizationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IHeadOfficeRepository, HeadOfficeRepository>();
            services.AddScoped<IHeadOfficeTenantWriter, HeadOfficeTenantWriter>();
            services.AddScoped<IBranchOfficeTenantWriter, BranchOfficeTenantWriter>();   
            services.AddScoped<HeadOfficeSystem>();
            services.AddScoped<IHeadOfficeDatabaseRestorer, HeadOfficeDatabaseRestorer>();
            services.AddScoped<IBranchOfficeRepository, BranchOfficeRepository>();
            services.AddScoped<BranchOfficeSystem>();
            services.AddScoped<IBranchLocationRepository, BranchLocationRepository>();
            services.AddScoped<BranchLocationSystem>();

            return services;
        }
    }
}