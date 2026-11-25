/*  Class Name      : GoverningBodyMappingActionResult.cs
 *  Purpose         : Result of Map Governing Bodies save / unmap
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies
{
    public class GoverningBodyMappingActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public int MappedCount { get; set; }
    }
}
