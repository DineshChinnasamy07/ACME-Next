namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Project
{
    public class ProjectSaveRequest
    {
        public int ProjectId { get; set; }

        public string? ProjectCode { get; set; }

        public string? ProjectName { get; set; }

        public int DivisionId { get; set; }

        public int ProjectCategroyId { get; set; }

        public int SocietyId { get; set; }

        public DateTime? AccountDate { get; set; }

        public DateTime? StartedOn { get; set; }

        public DateTime? ClosedOn { get; set; }

        public string? Description { get; set; }

        public string? Notes { get; set; }
    }
}
