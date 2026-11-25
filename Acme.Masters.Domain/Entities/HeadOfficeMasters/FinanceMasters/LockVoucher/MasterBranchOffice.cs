/*  Class Name      : MasterBranchOffice.cs
 *  Purpose         : HO tenant BRANCH_OFFICE lookup row for Lock Voucher
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class MasterBranchOffice
    {
        public int BranchOfficeId { get; set; }

        public string BranchOfficeCode { get; set; } = string.Empty;

        public string BranchOfficeName { get; set; } = string.Empty;

        public int Status { get; set; }
    }
}
