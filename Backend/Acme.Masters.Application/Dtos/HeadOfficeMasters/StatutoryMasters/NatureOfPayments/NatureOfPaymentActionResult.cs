/*  Class Name      : NatureOfPaymentActionResult.cs
 *  Purpose         : Save / delete outcome for a Nature of Payment
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public class NatureOfPaymentActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int NaturePayId { get; set; }
    }
}
