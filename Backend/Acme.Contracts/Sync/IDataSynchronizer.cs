/*  Class Name      : IDataSynchronizer.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using Acme.Common;

namespace Acme.Contracts.Sync
{
    public interface IDataSynchronizer
    {
        ResultArgs SynchronizeVouchers(string voucherXml);
    }
}
