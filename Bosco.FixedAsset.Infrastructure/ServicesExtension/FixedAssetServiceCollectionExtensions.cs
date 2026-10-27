/*  Class Name      : FixedAssetServiceCollectionExtensions.cs
 *  Purpose         : Register Fixed Asset services
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.FixedAsset.Infrastructure.Extensions
{
    public static class FixedAssetServiceCollectionExtensions
    {
        public static IServiceCollection AddFixedAssetServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
