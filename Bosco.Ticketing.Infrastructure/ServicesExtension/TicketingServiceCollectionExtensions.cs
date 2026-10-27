/*  Class Name      : TicketingServiceCollectionExtensions.cs
 *  Purpose         : Register Ticketing services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Ticketing.Infrastructure.Extensions
{
    public static class TicketingServiceCollectionExtensions
    {
        public static IServiceCollection AddTicketingServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
