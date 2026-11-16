/*  Class Name      : MasterProject.cs
 *  Purpose         : HO tenant MASTER_PROJECT row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project
{
    public class MasterProject
    {
        public int ProjectId { get; set; }

        public string? ProjectCode { get; set; }

        public string Project { get; set; } = string.Empty;

        public int DivisionId { get; set; }

        public DateTime? AccountDate { get; set; }

        public DateTime? DateStarted { get; set; }

        public DateTime? DateClosed { get; set; }

        public int ClosedBy { get; set; }

        public string? Description { get; set; }

        public string? Notes { get; set; }

        public int ProjectCategoryId { get; set; }

        public int DeleteFlag { get; set; }

        public int CustomerId { get; set; }

        public int ContributionId { get; set; }
    }
}
