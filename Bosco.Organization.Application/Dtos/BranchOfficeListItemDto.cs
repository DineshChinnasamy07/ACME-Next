/*  Class Name      : BranchOfficeListItemDto.cs
 *  Purpose         : One row for Branch Office list (FetchBranchOfficeDetails)
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class BranchOfficeListItemDto
    {
        public int BranchOfficeId { get; set; }

        public string HeadOfficeCode { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int DeploymentType { get; set; }

        public string SystemType { get; set; } = string.Empty;

        public int Status { get; set; }

        public string OfficeStatus { get; set; } = string.Empty;

        public int IsSubBranch { get; set; }

        public string SubBranch { get; set; } = string.Empty;
    }
}
