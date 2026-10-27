/*  Class Name      : SerilogExtensions.cs
 *  Purpose         : Shared file logging for all ACPP services
 *  Author          : Dinesh C
 *  Created on      : 19-Oct-2026
 */
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Bosco.Common.Logging
{
    public static class SerilogExtensions
    {
        public static IHostBuilder UseBoscoSerilog(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((context, configuration) =>
            {
                string serviceName = context.HostingEnvironment.ApplicationName
                    .Replace("Bosco.", "")
                    .Replace(".Api", "")
                    .Replace(".", "")
                    .ToLowerInvariant();

                string logFolder = Path.Combine(context.HostingEnvironment.ContentRootPath, "serilogs");
                Directory.CreateDirectory(logFolder);

                configuration
                    .MinimumLevel.Information()
                    .WriteTo.File(
                        Path.Combine(logFolder, serviceName + "-log-.txt"),
                        rollingInterval: RollingInterval.Day,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1)
                    );
            });
        }
    }
}
