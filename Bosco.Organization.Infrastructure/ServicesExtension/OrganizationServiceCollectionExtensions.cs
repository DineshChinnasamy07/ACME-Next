/*  Class Name      : OrganizationServiceCollectionExtensions.cs
 *  Purpose         : Register Organization services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Organization.Infrastructure.Extensions
{
    public static class OrganizationServiceCollectionExtensions
    {
        public static IServiceCollection AddOrganizationServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
