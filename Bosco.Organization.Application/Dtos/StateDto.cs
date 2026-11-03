/*  Class Name      : StateDto.cs
 *  Purpose         : One row for state dropdown (FetchStateByCountry)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class StateDto
    {
        public int StateId { get; set; }
        public string State { get; set; } = string.Empty;
        public string? StateCode { get; set; }

    }
}
