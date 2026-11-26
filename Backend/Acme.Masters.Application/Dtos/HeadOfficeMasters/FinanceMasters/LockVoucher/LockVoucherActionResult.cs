namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class LockVoucherActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int LockTransId { get; set; }
    }
}
