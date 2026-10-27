/*  Class Name      : ReportsServiceCollectionExtensions.cs
 *  Purpose         : Register Reports services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Reports.Infrastructure.Extensions
{
    public static class ReportsServiceCollectionExtensions
    {
        public static IServiceCollection AddReportsServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
