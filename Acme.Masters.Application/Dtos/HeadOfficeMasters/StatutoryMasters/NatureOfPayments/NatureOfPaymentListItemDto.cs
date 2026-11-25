/*  Class Name      : NatureOfPaymentListItemDto.cs
 *  Purpose         : Nature of Payment grid row / detail returned by api/natureofpayments
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public class NatureOfPaymentListItemDto
    {
        public int NaturePayId { get; set; }

        public string PaymentCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int TdsSectionId { get; set; }

        public string SectionCode { get; set; } = string.Empty;

        // Section name from TDS_SECTION (legacy grid column "Section")
        public string SectionName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // 1 = Active, 0 = Inactive
        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;
    }
}
