namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class ProjectCategoryListItemDto
    {
        public int ProjectCatogoryId { get; set; }

        public string ProjectCatogoryName { get; set; } = string.Empty;

        public int ProjectCatogoryGroupId { get; set; }

        public string Group { get; set; } = string.Empty;

        public int ProjectCatogoryItrgroupId { get; set; }

        public string ItrGroup { get; set; } = string.Empty;
    }
}
