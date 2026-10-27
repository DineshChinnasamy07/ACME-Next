/*  Class Name      : DatabaseServiceCollectionExtensions.cs
 *  Purpose         : Register the shared PostgreSQL connection for every ACPP service
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using Bosco.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Data
{
    public static class DatabaseServiceCollectionExtensions
    {
        public const string ConnectionName = "AcpErp";

        public static IServiceCollection AddBoscoDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString(ConnectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'AcpErp' is missing. Set ConnectionStrings:AcpErp in appsettings.json.");
            }

            services.AddScoped<IDatabase>(serviceProvider =>
            {
                IDatabaseQuery databaseQuery = serviceProvider.GetService<IDatabaseQuery>();
                return new PostgreSqlDatabase(connectionString, databaseQuery);
            });

            return services;
        }
    }
}
