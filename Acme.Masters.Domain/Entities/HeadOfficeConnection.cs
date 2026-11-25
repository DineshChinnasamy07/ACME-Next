/*  Class Name      : HeadOfficeConnection.cs
 *  Purpose         : Portal head_office connection fields to open the HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Domain.Entities
{
    public class HeadOfficeConnection
    {
        public int HeadOfficeId { get; set; }

        public string HeadOfficeCode { get; set; } = string.Empty;

        public string? HostName { get; set; }

        public string? DbName { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }
    }
}
