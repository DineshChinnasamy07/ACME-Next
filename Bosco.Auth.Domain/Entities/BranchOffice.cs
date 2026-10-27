/*  Class Name      : BranchOffice.cs
 *  Purpose         : Branch name joined from BRANCH_CODE during AuthenticateUser
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
namespace Bosco.Auth.Domain.Entities
{
    public class BranchOffice
    {
        public int BranchOfficeId { get; set; }

        public string BranchOfficeCode { get; set; } = string.Empty;

        public string BranchOfficeName { get; set; } = string.Empty;
    }
}
