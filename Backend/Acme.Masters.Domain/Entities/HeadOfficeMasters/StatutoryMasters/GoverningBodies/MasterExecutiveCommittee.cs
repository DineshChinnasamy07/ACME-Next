/*  Class Name      : MasterExecutiveCommittee.cs
 *  Purpose         : HO tenant MASTER_EXECUTIVE_COMMITTEE row (Governing Body member)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.GoverningBodies
{
    public class MasterExecutiveCommittee
    {
        public int ExecutiveId { get; set; }

        public string Executive { get; set; } = string.Empty;

        public string? Name { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Religion { get; set; }

        public string? Role { get; set; }

        public string Nationality { get; set; } = string.Empty;

        public string? Occupation { get; set; }

        public string? Association { get; set; }

        public string? OfficeBearer { get; set; }

        public string? Place { get; set; }

        public int? CountryId { get; set; }

        public int? StateId { get; set; }

        public string? Address { get; set; }

        public string? PinCode { get; set; }

        public string? PanSsn { get; set; }

        public string? Phone { get; set; }

        public string? Fax { get; set; }

        public string? Email { get; set; }

        public string? Url { get; set; }

        public DateTime? DateOfAppointment { get; set; }

        public DateTime? DateOfExit { get; set; }

        public string? Notes { get; set; }
    }
}
