/*  Class Name      : TdsSectionSaveRequest.cs
 *  Purpose         : Insert / update request for a TDS Section (TdsSectionId 0 = insert)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsSection
{
    public class TdsSectionSaveRequest
    {
        public int TdsSectionId { get; set; }

        public string? Code { get; set; }

        public string? SectionName { get; set; }

        // 1 = Active, 0 = Inactive
        public int Status { get; set; } = 1;
    }
}
