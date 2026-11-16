/*  Class Name      : MasterLockTrans.cs
 *  Purpose         : HO tenant MASTER_LOCK_TRANS (Lock Voucher) row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class MasterLockTrans
    {
        public int LockTransId { get; set; }

        public int BranchId { get; set; }

        public int ProjectId { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public string Password { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string? PasswordHint { get; set; }

        public string? LockType { get; set; }

        public int? LockByPortal { get; set; }
    }
}
