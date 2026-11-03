/*  Class Name      : BranchOffice.cs
 *  Purpose         : Portal branch_office row for list, get, and SaveBranchOfficeDetails
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using System;

namespace Bosco.Organization.Domain.Entities
{
    public class BranchOffice
    {
        public int BranchOfficeId { get; set; }

        public string BranchOfficeCode { get; set; } = string.Empty;

        public string BranchOfficeName { get; set; } = string.Empty;

        public string HeadOfficeCode { get; set; } = string.Empty;

        public string? BranchPartCode { get; set; }

        public int? DeploymentType { get; set; }

        public int Status { get; set; }

        public int IsSubBranch { get; set; }

        public string? Address { get; set; }

        public int? CountryId { get; set; }

        public int? StateId { get; set; }

        public string? Pincode { get; set; }

        public string? PhoneNo { get; set; }

        public string? MobileNo { get; set; }

        public string? CountryCode { get; set; }

        public string? BranchEmail { get; set; }

        public string? City { get; set; }

        public int? UserCreatedStatus { get; set; }

        public string? BranchKeyCode { get; set; }

        public string? PersonIncharge { get; set; }

        public string? ThirdPartyCode { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
