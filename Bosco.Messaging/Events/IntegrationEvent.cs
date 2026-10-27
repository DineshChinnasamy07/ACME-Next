/*  Class Name      : IntegrationEvent.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;

namespace Bosco.Messaging.Events
{
    public abstract class IntegrationEvent
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string EventType { get; set; }
        public string Source { get; set; }

        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTime.UtcNow;
            EventType = GetType().Name;
        }
    }
}
