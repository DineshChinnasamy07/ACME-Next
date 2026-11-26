/*  Class Name      : MappingBranchUser.cs
 *  Purpose         : HO tenant BRANCH_USER row (user mapped to extra branches)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapProjects
{
    public class MappingBranchUser
    {
        public int BranchUserId { get; set; }

        public int BranchId { get; set; }

        public int UserId { get; set; }

        public int UserType { get; set; }
    }
}
