/*  Class Name      : MasterProjectCatogory.cs
 *  Purpose         : HO tenant MASTER_PROJECT_CATOGORY row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class MasterProjectCatogory
    {
        public int ProjectCatogoryId { get; set; }

        public string? ProjectCatogoryName { get; set; }

        public int? ProjectCatogoryGroupId { get; set; }

        public int? ProjectCatogoryItrgroupId { get; set; }
    }
}
