/*  Class Name      : ProjectBranchMapping.cs
 *  Purpose         : HO tenant PROJECT_BRANCH_MAPPING row: one project mapped to one branch with its location, society, category, division, period and user in charge
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectBranchMapping
    {
        public int ProjectBranchMapId { get; set; }

        public int ProjectId { get; set; }

        public int BranchId { get; set; }

        public int LocationId { get; set; }

        public int CustomerId { get; set; }

        public int ProjectCategoryId { get; set; }

        public int DivisionId { get; set; }

        public DateTime DateStarted { get; set; }

        public DateTime? DateClosed { get; set; }

        public int? UserId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
