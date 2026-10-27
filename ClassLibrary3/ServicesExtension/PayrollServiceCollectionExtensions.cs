/*  Class Name      : PayrollServiceCollectionExtensions.cs
 *  Purpose         : Register Payroll services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Payroll.Infrastructure.Extensions
{
    public static class PayrollServiceCollectionExtensions
    {
        public static IServiceCollection AddPayrollServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
