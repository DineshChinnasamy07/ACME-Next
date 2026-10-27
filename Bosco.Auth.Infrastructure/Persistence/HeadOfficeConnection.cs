/*  Class Name      : HeadOfficeConnection.cs
 *  Purpose         : Build the PostgreSQL connection for one head office database
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Domain.Entities;
using Bosco.Data;
using Microsoft.Extensions.Configuration;

namespace Bosco.Auth.Infrastructure.Persistence
{
    public static class HeadOfficeConnection
    {
        public static string Build(HeadOffice office, IConfiguration configuration)
        {
            string host = office.HostName == null ? string.Empty : office.HostName.Trim();
            int port = Port(configuration);
            int separator = host.LastIndexOf(':');
            if (separator > 0 && int.TryParse(host.Substring(separator + 1), out int parsedPort))
            {
                host = host.Substring(0, separator);
                port = parsedPort;
            }

            return "Host=" + host
                + ";Port=" + port.ToString()
                + ";Database=" + office.DbName.Trim()
                + ";Username=" + (office.UserName ?? string.Empty)
                + ";Password=" + (office.Password ?? string.Empty)
                + ";Timeout=5;Command Timeout=5";
        }

        private static int Port(IConfiguration configuration)
        {
            string portal = configuration.GetConnectionString(DatabaseServiceCollectionExtensions.ConnectionName) ?? string.Empty;
            string[] parts = portal.Split(';');
            for (int index = 0; index < parts.Length; index++)
            {
                string[] pair = parts[index].Split(new char[] { '=' }, 2);
                if (pair.Length == 2 && string.Equals(pair[0].Trim(), "Port", StringComparison.OrdinalIgnoreCase))
                {
                    int port;
                    if (int.TryParse(pair[1].Trim(), out port))
                    {
                        return port;
                    }
                }
            }

            return 3308;
        }
    }
}
