/*  Class Name      : NatureOfPaymentSaveRequest.cs
 *  Purpose         : Insert / update request for a Nature of Payment (NaturePayId 0 = insert)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public class NatureOfPaymentSaveRequest
    {
        public int NaturePayId { get; set; }

        public string? Name { get; set; }

        public string? PaymentCode { get; set; }

        public int TdsSectionId { get; set; }

        public string? Description { get; set; }

        // 1 = Active, 0 = Inactive
        public int Status { get; set; } = 1;
    }
}
