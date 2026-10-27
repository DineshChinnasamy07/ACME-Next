/*  Class Name      : FinanceServiceCollectionExtensions.cs
 *  Purpose         : Register Finance services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Finance.Infrastructure.Extensions
{
    public static class FinanceServiceCollectionExtensions
    {
        public static IServiceCollection AddFinanceServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
