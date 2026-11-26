/*  Class Name      : GoverningBodyMappingRowDto.cs
 *  Purpose         : Map Governing Bodies grid row (legacy FetchGoverningMemberByLegalEntity)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies
{
    public class GoverningBodyMappingRowDto
    {
        public int ExecutiveId { get; set; }

        public string Executive { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime? DateOfAppointment { get; set; }

        public DateTime? DateOfExit { get; set; }

        public bool Selected { get; set; }
    }
}
