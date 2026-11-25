namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Project
{
    public class ProjectListItemDto
    {
        public int ProjectId { get; set; }

        public string ProjectCode { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public int DivisionId { get; set; }

        public string Division { get; set; } = string.Empty;

        public int ProjectCategoryId { get; set; }

        public string Category { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public string Society { get; set; } = string.Empty;

        public DateTime? DateStarted { get; set; }

        public DateTime? DateClosed { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
