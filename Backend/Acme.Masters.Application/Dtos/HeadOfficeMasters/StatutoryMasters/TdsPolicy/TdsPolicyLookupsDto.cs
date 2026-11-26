/*  Class Name      : TdsPolicyLookupsDto.cs
 *  Purpose         : TDS Policy form lookups (active deductee types, nature of payments, duty / tax types)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsPolicyLookupsDto
    {
        public List<TdsPolicyOptionDto> DeducteeTypes { get; set; } = new List<TdsPolicyOptionDto>();

        public List<TdsPolicyOptionDto> NatureOfPayments { get; set; } = new List<TdsPolicyOptionDto>();

        public List<TdsPolicyOptionDto> TaxTypes { get; set; } = new List<TdsPolicyOptionDto>();
    }
}
