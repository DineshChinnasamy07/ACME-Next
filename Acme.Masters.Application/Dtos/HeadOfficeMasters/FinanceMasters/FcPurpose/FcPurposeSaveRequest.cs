namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.FcPurpose
{
    public class FcPurposeSaveRequest
    {
        public int ContributionId { get; set; }

        public string? Code { get; set; }

        public string? FcPurpose { get; set; }
    }
}
