/*  Class Name      : MasterTdsDutyTaxType.cs
 *  Purpose         : HO tenant TDS_DUTY_TAXTYPE row (TDS duty / tax type master)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.DutyTax
{
    public class MasterTdsDutyTaxType
    {
        public int TdsDutyTaxTypeId { get; set; }
        public string TaxTypeName { get; set; } = string.Empty;
        public int Status { get; set; }
    }
}
