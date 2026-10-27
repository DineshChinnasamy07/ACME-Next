/*  Class Name      : HeadOffice.cs
 *  Purpose         : Portal head_office row that names the head office database
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
namespace Bosco.Auth.Domain.Entities
{
    public class HeadOffice
    {
        public int HeadOfficeId { get; set; }

        public string HeadOfficeCode { get; set; } = string.Empty;

        public string HeadOfficeName { get; set; } = string.Empty;

        public string HostName { get; set; } = string.Empty;

        public string DbName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
