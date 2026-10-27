/*  Class Name      : InMemoryEventBus.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bosco.Common;
using Bosco.Messaging.Abstractions;
using Bosco.Messaging.Events;

namespace Bosco.Messaging
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Type>> handlers = new Dictionary<Type, List<Type>>();

        public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            if (integrationEvent == null)
            {
                return Task.CompletedTask;
            }

            AcMELog.WriteLog("Publish: " + integrationEvent.EventType + " / " + integrationEvent.Id);
            return Task.CompletedTask;
        }

        public void Subscribe<TEvent, THandler>()
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            Type eventType = typeof(TEvent);
            if (!handlers.ContainsKey(eventType))
            {
                handlers[eventType] = new List<Type>();
            }

            Type handlerType = typeof(THandler);
            if (!handlers[eventType].Contains(handlerType))
            {
                handlers[eventType].Add(handlerType);
            }
        }

        public void Unsubscribe<TEvent, THandler>()
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            Type eventType = typeof(TEvent);
            if (handlers.ContainsKey(eventType))
            {
                handlers[eventType].Remove(typeof(THandler));
            }
        }
    }
}
