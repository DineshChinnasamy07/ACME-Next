/*  Class Name      : UpdateDatabaseRequest.cs
 *  Purpose         : UpdateDatabase body (HOST_NAME / DB_NAME / USERNAME / PASSWORD)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */

namespace Bosco.Organization.Application.Dtos
{
    public class UpdateDatabaseRequest
    {
        public string? HostName { get; set; }
        public string? DbName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
