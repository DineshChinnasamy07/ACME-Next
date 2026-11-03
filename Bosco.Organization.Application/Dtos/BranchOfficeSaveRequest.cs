/*  Class Name      : BranchOfficeSaveRequest.cs
 *  Purpose         : SaveBranchOfficeDetails request body (add when BranchOfficeId is 0)
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class BranchOfficeSaveRequest
    {
        public int BranchOfficeId { get; set; }

        public string HeadOfficeCode { get; set; } = string.Empty;

        public string BranchPartCode { get; set; } = string.Empty;

        public string BranchOfficeName { get; set; } = string.Empty;

        public int? DeploymentType { get; set; }

        public string? PersonIncharge { get; set; }

        public int IsSubBranch { get; set; }

        public string? CountryCode { get; set; }

        public string? MobileNo { get; set; }

        public string? PhoneNo { get; set; }

        public string? BranchEmail { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public int? CountryId { get; set; }

        public int? StateId { get; set; }

        public string? Pincode { get; set; }

        public string? ThirdPartyCode { get; set; }
    }
}
