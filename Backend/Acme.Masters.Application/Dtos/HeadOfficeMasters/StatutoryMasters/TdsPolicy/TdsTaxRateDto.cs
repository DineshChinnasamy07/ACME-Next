/*  Class Name      : TdsTaxRateDto.cs
 *  Purpose         : One TDS_TAX_RATE row of a policy (with the duty / tax type name)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsTaxRateDto
    {
        public int TaxRateId { get; set; }

        public int TdsTaxTypeId { get; set; }

        public string TaxTypeName { get; set; } = string.Empty;

        public decimal TdsRate { get; set; }

        public decimal TdsExemptionLimit { get; set; }
    }
}
