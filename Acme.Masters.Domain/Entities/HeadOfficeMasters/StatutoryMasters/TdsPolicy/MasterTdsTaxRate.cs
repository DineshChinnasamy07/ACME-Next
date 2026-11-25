/*  Class Name      : MasterTdsTaxRate.cs
 *  Purpose         : HO tenant TDS_TAX_RATE row (rate % and exemption limit of one duty / tax type in a TDS policy)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class MasterTdsTaxRate
    {
        public int TaxRateId { get; set; }
        public int TdsPolicyId { get; set; }
        public decimal TdsRate { get; set; }
        public decimal TdsExemptionLimit { get; set; }
        public int TdsTaxTypeId { get; set; }
    }
}
