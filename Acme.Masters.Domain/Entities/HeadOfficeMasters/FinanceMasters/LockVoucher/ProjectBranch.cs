/*  Class Name      : ProjectBranch.cs
 *  Purpose         : HO tenant PROJECT_BRANCH mapping row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class ProjectBranch
    {
        public int ProjectId { get; set; }

        public int BranchId { get; set; }

        public int LocationId { get; set; }
    }
}
