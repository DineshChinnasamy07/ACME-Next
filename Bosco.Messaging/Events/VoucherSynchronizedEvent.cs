/*  Class Name      : VoucherSynchronizedEvent.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
namespace Bosco.Messaging.Events
{
    public class VoucherSynchronizedEvent : IntegrationEvent
    {
        public string VoucherXml { get; set; }
        public string HeadOfficeCode { get; set; }
        public string BranchOfficeCode { get; set; }
        public bool Success { get; set; }
    }
}
