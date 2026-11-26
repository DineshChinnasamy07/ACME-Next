/*  Class Name      : IPortalMessagePublisher.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Threading;
using System.Threading.Tasks;
using Acme.Common;
using Acme.Messaging.Events;

namespace Acme.Messaging.Abstractions
{
    public interface IPortalMessagePublisher
    {
        Task<ResultArgs> PublishAsync(PortalMessage message, CancellationToken cancellationToken = default);
    }
}
