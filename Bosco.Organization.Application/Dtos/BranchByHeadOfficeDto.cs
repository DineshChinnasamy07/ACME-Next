/*  Class Name      : BranchByHeadOfficeDto.cs
 *  Purpose         : Active main branch under HO (FetchBranchByHeadOffice)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class BranchByHeadOfficeDto
    {
        public int BranchOfficeId { get; set; }

        public string BranchOfficeCode { get; set; } = string.Empty;

        public string BranchOfficeName { get; set; } = string.Empty;
    }
}
