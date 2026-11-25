/*  Class Name      : MasterTdsSection.cs
 *  Purpose         : HO tenant TDS_SECTION row (TDS section master)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsSection
{
    public class MasterTdsSection
    {
        public int TdsSectionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public int Status { get; set; }
    }
}
