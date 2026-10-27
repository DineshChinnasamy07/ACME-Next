/*  Class Name      : IIntegrationEventHandler.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Threading;
using System.Threading.Tasks;
using Bosco.Messaging.Events;

namespace Bosco.Messaging.Abstractions
{
    public interface IIntegrationEventHandler<in TEvent> where TEvent : IntegrationEvent
    {
        Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
