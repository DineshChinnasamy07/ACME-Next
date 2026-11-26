/*  Class Name      : TdsPolicySaveRequest.cs
 *  Purpose         : TDS Policy save request (policy + tax rate rows; TdsPolicyId 0 = new)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsPolicySaveRequest
    {
        public int TdsPolicyId { get; set; }

        public int DeducteeTypeId { get; set; }

        public int NaturePayId { get; set; }

        public DateTime? ApplicableFrom { get; set; }

        public List<TdsTaxRateSaveRow>? TaxRates { get; set; }
    }

    public class TdsTaxRateSaveRow
    {
        public int TdsTaxTypeId { get; set; }

        public decimal TdsRate { get; set; }

        public decimal TdsExemptionLimit { get; set; }
    }
}
