/*  Class Name      : MasterContributionHead.cs
 *  Purpose         : HO tenant MASTER_CONTRIBUTION_HEAD (FC Purpose) row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.FcPurpose
{
    public class MasterContributionHead
    {
        public int ContributionId { get; set; }

        public string? Code { get; set; }

        public string? FcPurpose { get; set; }
    }
}
