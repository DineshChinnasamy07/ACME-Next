/*  Class Name      : PortalMessage.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;

namespace Bosco.Messaging.Events
{
    public class PortalMessage : IntegrationEvent
    {
        public int ProjectId { get; set; }
        public string Project { get; set; }
        public string MessageType { get; set; }
        public string Payload { get; set; }
        public DateTime? TransDateFrom { get; set; }
        public DateTime? TransDateTo { get; set; }
        public string Status { get; set; }
    }
}
