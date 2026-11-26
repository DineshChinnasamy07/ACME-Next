namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class GeneralateLedgerSaveRequest
    {
        public int ConLedgerId { get; set; }

        public string? ConLedgerCode { get; set; }

        public string? ConLedgerName { get; set; }

        public int ConParentLedgerId { get; set; }
    }
}
