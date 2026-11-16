namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class LockVoucherSaveRequest
    {
        public int LockTransId { get; set; }

        public int BranchId { get; set; }

        public int ProjectId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? Password { get; set; }

        public string? PasswordHint { get; set; }

        public string? Reason { get; set; }
    }
}
