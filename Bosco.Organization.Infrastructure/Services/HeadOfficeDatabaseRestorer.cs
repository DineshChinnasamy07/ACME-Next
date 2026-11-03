/*  Class Name      : HeadOfficeDatabaseRestorer.cs
 *  Purpose         : PostgreSQL create DB + run HO template script (legacy RestoreACPERPdatabase)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using System.Text;
using System.Text.RegularExpressions;
using Bosco.Organization.Application.Abstractions.Services;
using Bosco.Organization.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Bosco.Organization.Infrastructure.Services
{
    public class HeadOfficeDatabaseRestorer : IHeadOfficeDatabaseRestorer
    {
        private readonly IConfiguration _configuration;

        public HeadOfficeDatabaseRestorer(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        #region Propertys
        private int ReadPort()
        {
            string portText = _configuration["HeadOfficeTenant:Port"] ?? "3308";
            int port;
            return int.TryParse(portText, out port) && port > 0 ? port : 3308;
        }
        private static HeadOfficeActionResult Ok(string message)
        {
            return new HeadOfficeActionResult { Success = true, Message = message };
        }
        private static HeadOfficeActionResult Fail(string message)
        {
            return new HeadOfficeActionResult { Success = false, Message = message };
        }


        #endregion


        public async Task<HeadOfficeActionResult> RestoreACPERPdatabase(string hostName, string databaseName, string userName, string password, CancellationToken cancellationToken)
        {
            string host = (hostName ?? string.Empty).Trim();
            string db = (databaseName ?? string.Empty).Trim().ToLowerInvariant();
            string user = (userName ?? string.Empty).Trim();
            string pwd = password ?? string.Empty;

            if (host.Length == 0 || db.Length == 0 || user.Length == 0)
            {
                return Fail("HostName, DbName, and UserName are required to create HO database.");
            }
            if (!Regex.IsMatch(db, "^[a-z][a-z0-9_]*$")) 
            {
                return Fail("DbName must start with a letter and use only letters, numbers, underscore.");
            }

            int port = ReadPort();
            string maintenanceDb = _configuration["HeadOfficeTenant:MaintenanceDatabase"] ?? "postgres";
            string maintenanceCs = BuildCs(host, port, maintenanceDb, user, pwd);
            try
            {
                bool exists = await DatabaseExists(maintenanceCs, db, cancellationToken);
                if (exists) 
                {
                    return Ok("HO database already exists.");
                }

                await using (NpgsqlConnection admin = new NpgsqlConnection(maintenanceCs))
                {
                    await admin.OpenAsync(cancellationToken);
                    await using (NpgsqlCommand create = new NpgsqlCommand("CREATE DATABASE \"" + db + "\"", admin))
                    {
                        await create.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                string templatePath = _configuration["HeadOfficeTenant:TemplateSqlPath"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(templatePath) || !File.Exists(templatePath))
                {
                    return Fail("HO database created, but TemplateSqlPath is missing or file not found: " + templatePath);
                }
                string sql = await File.ReadAllTextAsync(templatePath, Encoding.UTF8, cancellationToken);
                sql = sql.Replace("@DB@", db);
                string tenantCs = BuildCs(host, port, db, user, pwd);
                await using (NpgsqlConnection tenant = new NpgsqlConnection(tenantCs))
                {
                    await tenant.OpenAsync(cancellationToken);
                    await using (NpgsqlCommand cmd = new NpgsqlCommand(sql, tenant))
                    {
                        cmd.CommandTimeout = 0;
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                return Ok("HO database created and template restored.");
            }
            catch (Exception ex)
            {

                return Fail("HO database restore failed: " + ex.Message);
            }
        }

        private static async Task<bool> DatabaseExists(string maintenanceCs, string databaseName, CancellationToken cancellationToken)
        {
            await using (NpgsqlConnection conn = new NpgsqlConnection(maintenanceCs))
            {
                await conn.OpenAsync(cancellationToken);
                await using (NpgsqlCommand cmd = new NpgsqlCommand(
                    "SELECT 1 FROM pg_database WHERE datname = @name", conn))
                {
                    cmd.Parameters.AddWithValue("name", databaseName);
                    object? value = await cmd.ExecuteScalarAsync(cancellationToken);
                    return value != null && value != DBNull.Value;
                }
            }
        }

        private static string BuildCs(string host, int port, string database, string user, string password)
        {
            return "Host=" + host
                + ";Port=" + port.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ";Database=" + database
                + ";Username=" + user
                + ";Password=" + password;
        }
    }
}
