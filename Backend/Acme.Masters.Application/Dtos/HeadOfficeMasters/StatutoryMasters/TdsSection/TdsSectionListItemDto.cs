/*  Class Name      : TdsSectionListItemDto.cs
 *  Purpose         : TDS Section grid row / detail returned by api/tdssections
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsSection
{
    public class TdsSectionListItemDto
    {
        public int TdsSectionId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        // 1 = Active, 0 = Inactive (legacy TDS_SECTION.STATUS)
        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;
    }
}
