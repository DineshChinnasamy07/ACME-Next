namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class ProjectCategorySaveRequest
    {
        public int ProjectCatogoryId { get; set; }

        public string? ProjectCatogoryName { get; set; }

        public int GeneralateCategoryId { get; set; }

        public int ITRGroupId { get; set; }
    }
}
