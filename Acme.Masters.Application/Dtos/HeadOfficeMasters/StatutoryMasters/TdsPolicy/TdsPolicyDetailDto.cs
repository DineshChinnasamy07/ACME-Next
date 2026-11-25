/*  Class Name      : TdsPolicyDetailDto.cs
 *  Purpose         : TDS Policy with its tax rate rows (edit form)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsPolicyDetailDto
    {
        public int TdsPolicyId { get; set; }

        public int DeducteeTypeId { get; set; }

        public int NaturePayId { get; set; }

        public DateTime ApplicableFrom { get; set; }

        public List<TdsTaxRateDto> TaxRates { get; set; } = new List<TdsTaxRateDto>();
    }
}
