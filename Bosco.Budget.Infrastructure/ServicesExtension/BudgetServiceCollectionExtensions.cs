/*  Class Name      : BudgetServiceCollectionExtensions.cs
 *  Purpose         : Register Budget services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Budget.Infrastructure.Extensions
{
    public static class BudgetServiceCollectionExtensions
    {
        public static IServiceCollection AddBudgetServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
