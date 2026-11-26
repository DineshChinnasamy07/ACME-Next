/*  Class Name      : MasterTdsPolicy.cs
 *  Purpose         : HO tenant TDS_POLICY row (deductee type + nature of payment + applicable from)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class MasterTdsPolicy
    {
        public int TdsPolicyId { get; set; }
        public int TdsDeducteeTypeId { get; set; }
        public int TdsNaturePaymentId { get; set; }
        public DateTime ApplicableFrom { get; set; }
    }
}
