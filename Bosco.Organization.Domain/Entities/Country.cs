/*  Class Name      : Country.cs
 *  Purpose         : Portal country row for country dropdown
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
namespace Bosco.Organization.Domain.Entities
{
    public class Country
    {
        public int CountryId { get; set; }

        public string? CountryCode { get; set; }

        public string CountryName { get; set; } = string.Empty;
    }
}
