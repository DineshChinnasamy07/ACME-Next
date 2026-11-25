namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class ProjectCategoryActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int ProjectCatogoryId { get; set; }
    }
}
