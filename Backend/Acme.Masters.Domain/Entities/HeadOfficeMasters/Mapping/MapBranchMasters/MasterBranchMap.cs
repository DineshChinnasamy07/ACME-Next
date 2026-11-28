/*  Class Name      : MasterBranchMap.cs
 *  Purpose         : Generic "this master record is available to this branch" row on the HO tenant DB.
 *                    One table (public.master_branch_map) serves every master type (ledger, legal entity,
 *                    project category, ledger group, purpose, …) via MASTER_TYPE + MASTER_ID + BRANCH_ID.
 *                    A branch login then sees only the master records mapped to its branch.
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapBranchMasters
{
    public class MasterBranchMap
    {
        public int MasterBranchMapId { get; set; }

        public string MasterType { get; set; } = string.Empty;

        public int MasterId { get; set; }

        public int BranchId { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
