/*  Class Name      : GoverningBodyListItemDto.cs
 *  Purpose         : Governing Bodies grid row (legacy GoverningMemberView: Name, Father/Husband, Nationality, Occupation, State, Country)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.GoverningBodies
{
    public class GoverningBodyListItemDto
    {
        public int ExecutiveId { get; set; }

        public string Executive { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Nationality { get; set; } = string.Empty;

        public string Occupation { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public int CountryId { get; set; }

        public int StateId { get; set; }

        public DateTime? DateOfAppointment { get; set; }

        public DateTime? DateOfExit { get; set; }
    }
}
