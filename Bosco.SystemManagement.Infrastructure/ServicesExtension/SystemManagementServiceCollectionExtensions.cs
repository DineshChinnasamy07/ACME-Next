/*  Class Name      : SystemManagementServiceCollectionExtensions.cs
 *  Purpose         : Register System Management services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.SystemManagement.Infrastructure.Extensions
{
    public static class SystemManagementServiceCollectionExtensions
    {
        public static IServiceCollection AddSystemManagementServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
