/*  Class Name      : MappingBranchLocation.cs
 *  Purpose         : HO tenant BRANCH_LOCATION lookup row for Project Mapping
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapProjects
{
    public class MappingBranchLocation
    {
        public int LocationId { get; set; }

        public int BranchId { get; set; }

        public string LocationName { get; set; } = string.Empty;
    }
}
