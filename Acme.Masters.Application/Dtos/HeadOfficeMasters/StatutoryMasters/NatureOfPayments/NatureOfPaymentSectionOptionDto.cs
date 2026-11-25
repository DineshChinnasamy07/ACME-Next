/*  Class Name      : NatureOfPaymentSectionOptionDto.cs
 *  Purpose         : Active TDS section option for the Nature of Payment form (legacy FetchSectionCodes)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public class NatureOfPaymentSectionOptionDto
    {
        public int TdsSectionId { get; set; }

        public string Code { get; set; } = string.Empty;

        // "CODE - SECTION_NAME"
        public string Name { get; set; } = string.Empty;
    }
}
