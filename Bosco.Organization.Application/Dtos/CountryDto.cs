/*  Class Name      : CountryDto.cs
 *  Purpose         : One row for country dropdown (FetchCountry)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class CountryDto
    {
        public int CountryId { get; set; }

        public string? CountryCode { get; set; }

        public string Country { get; set; } = string.Empty;
    }
}
