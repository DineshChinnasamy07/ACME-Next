/*  Class Name      : GoverningBodyDetailDto.cs
 *  Purpose         : Governing Body member edit form data (legacy ExecutiveMemberSystem.FillExecutiveMember)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.GoverningBodies
{
    public class GoverningBodyDetailDto
    {
        public int ExecutiveId { get; set; }

        /// <summary>
        /// Member name (EXECUTIVE).
        /// </summary>
        public string? Executive { get; set; }

        /// <summary>
        /// Father / Husband name (NAME).
        /// </summary>
        public string? Name { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Religion { get; set; }

        public string? Role { get; set; }

        public string? Nationality { get; set; }

        public string? Occupation { get; set; }

        /// <summary>
        /// Office held in association (ASSOCIATION).
        /// </summary>
        public string? Association { get; set; }

        /// <summary>
        /// Relationship with office bearer (OFFICE_BEARER).
        /// </summary>
        public string? OfficeBearer { get; set; }

        public string? Place { get; set; }

        public int CountryId { get; set; }

        public int StateId { get; set; }

        public string? Address { get; set; }

        public string? PinCode { get; set; }

        public string? PanSsn { get; set; }

        /// <summary>
        /// Contact number (PHONE).
        /// </summary>
        public string? Phone { get; set; }

        public string? Fax { get; set; }

        public string? Email { get; set; }

        public string? Url { get; set; }

        /// <summary>
        /// Date of joining (DATE_OF_APPOINTMENT).
        /// </summary>
        public DateTime? DateOfAppointment { get; set; }

        public DateTime? DateOfExit { get; set; }

        public string? Notes { get; set; }
    }
}
