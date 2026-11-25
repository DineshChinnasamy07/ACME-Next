/*  Class Name      : MasterTdsNaturePayment.cs
 *  Purpose         : HO tenant TDS_NATURE_PAYMENT row (TDS nature of payment master)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public class MasterTdsNaturePayment
    {
        public int NaturePayId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string? SectionName { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TdsSectionId { get; set; }
        public int Status { get; set; }
    }
}
