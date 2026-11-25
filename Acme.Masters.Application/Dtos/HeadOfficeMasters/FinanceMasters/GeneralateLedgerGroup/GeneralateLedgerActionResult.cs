namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class GeneralateLedgerActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int ConLedgerId { get; set; }
    }
}
