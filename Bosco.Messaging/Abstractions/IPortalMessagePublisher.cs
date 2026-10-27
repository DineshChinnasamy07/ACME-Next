/*  Class Name      : IPortalMessagePublisher.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Threading;
using System.Threading.Tasks;
using Bosco.Common;
using Bosco.Messaging.Events;

namespace Bosco.Messaging.Abstractions
{
    public interface IPortalMessagePublisher
    {
        Task<ResultArgs> PublishAsync(PortalMessage message, CancellationToken cancellationToken = default);
    }
}
