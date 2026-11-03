/*  Class Name      : State.cs
 *  Purpose         : Portal state row for state dropdown by country
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */

namespace Bosco.Organization.Domain.Entities
{
    public class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string? StateCode { get; set; }
        public int CountryId { get; set; }
    }
}
