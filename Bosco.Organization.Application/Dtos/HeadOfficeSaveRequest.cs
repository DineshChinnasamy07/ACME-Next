/*  Class Name      : HeadOfficeSaveRequest.cs
 *  Purpose         : SaveHeadOfficeDetails request body (add when HeadOfficeId is 0)
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class HeadOfficeSaveRequest
    {
        public int HeadOfficeId { get; set; }

        public string HeadOfficeCode { get; set; } = string.Empty;

        public string? HeadOfficeName { get; set; }

        public int? TypeId { get; set; }

        public string? BelongsTo { get; set; }

        public string? Designation { get; set; }

        public string? OrgMailId { get; set; }

        public string? InchargeName { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? MobileNo { get; set; }

        public string? PhoneNo { get; set; }

        public int? CountryId { get; set; }

        public int? StateId { get; set; }

        public string? Pincode { get; set; }

        public string? CountryCode { get; set; }

        public string? SrInchargeName { get; set; }

        public string? SrMobileNo { get; set; }

        public string? SrPhoneNo { get; set; }

        public string? SrMailId { get; set; }

        public string? HostName { get; set; }

        public string? DbName { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public int AccountingYearType { get; set; }
    }
}
