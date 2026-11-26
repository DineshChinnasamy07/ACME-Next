/*  Class Name      : GoverningBodyMappingSaveRequest.cs
 *  Purpose         : Map Governing Bodies save request: the ticked members replace the legal entity's mapping
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies
{
    public class GoverningBodyMappingSaveRequest
    {
        public int CustomerId { get; set; }

        public List<int>? ExecutiveIds { get; set; }
    }
}
