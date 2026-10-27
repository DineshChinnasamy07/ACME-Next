/*  Class Name      : IDataSynchronizer.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using Bosco.Common;

namespace Bosco.Contracts.Sync
{
    public interface IDataSynchronizer
    {
        ResultArgs SynchronizeVouchers(string voucherXml);
    }
}
