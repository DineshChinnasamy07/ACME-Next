namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class GeneralateLedgerListItemDto
    {
        public int ConLedgerId { get; set; }

        public string ConLedgerCode { get; set; } = string.Empty;

        public string ConLedgerName { get; set; } = string.Empty;

        public int ConParentLedgerId { get; set; }

        public string ParentName { get; set; } = string.Empty;

        public int ConMainParentId { get; set; }

        public bool HasChildren { get; set; }
    }
}
