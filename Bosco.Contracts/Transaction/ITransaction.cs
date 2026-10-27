/*  Class Name      : ITransaction.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using Bosco.Common;

namespace Bosco.Contracts.Transaction
{
    public interface IVoucherMaster
    {
        string VoucherId { get; set; }
        ResultArgs SaveVoucherMaster();
        ResultArgs DeleteVoucherMaster();
        ResultArgs UpdateVoucherMaster();
        ResultArgs FetchVoucherMaster();
    }

    public interface IVoucherMasterDetails
    {
        string VoucherDetailId { get; set; }
        ResultArgs SaveVoucherMasterDetails();
        ResultArgs DeleteVoucherMasterDetails();
        ResultArgs UpdateVoucherMasterDetails();
        ResultArgs FetchVoucherMasterDetails();
    }
}
