namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class LockVoucherListItemDto
    {
        public int LockTransId { get; set; }

        public int BranchId { get; set; }

        public string BranchOffice { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        public string Project { get; set; } = string.Empty;

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public string Reason { get; set; } = string.Empty;

        public bool Locked { get; set; }
    }
}
