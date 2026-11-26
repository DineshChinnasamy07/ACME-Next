/*  Class Name      : TdsPolicyListItemDto.cs
 *  Purpose         : TDS Policy grid row (names resolved, tax rates included)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsPolicyListItemDto
    {
        public int TdsPolicyId { get; set; }

        public int DeducteeTypeId { get; set; }

        public string DeducteeType { get; set; } = string.Empty;

        public int NaturePayId { get; set; }

        public string NatureOfPayment { get; set; } = string.Empty;

        public string PaymentCode { get; set; } = string.Empty;

        public int TdsSectionId { get; set; }

        public string SectionCode { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public DateTime ApplicableFrom { get; set; }

        public List<TdsTaxRateDto> TaxRates { get; set; } = new List<TdsTaxRateDto>();
    }
}
