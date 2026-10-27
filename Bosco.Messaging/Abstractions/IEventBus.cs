/*  Class Name      : IEventBus.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Threading;
using System.Threading.Tasks;
using Bosco.Messaging.Events;

namespace Bosco.Messaging.Abstractions
{
    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
        void Subscribe<TEvent, THandler>()
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>;
        void Unsubscribe<TEvent, THandler>()
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>;
    }
}
