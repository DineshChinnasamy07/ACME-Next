namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Project
{
    public class ProjectActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int ProjectId { get; set; }
    }
}
