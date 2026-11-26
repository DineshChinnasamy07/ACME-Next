using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public interface ILockVoucherRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<LockVoucherListItemDto>> FetchLockedProjects(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<MasterLockTrans?> FillLockVoucherDetails(HeadOfficeConnection office, int lockTransId, CancellationToken cancellationToken);

        Task<IReadOnlyList<LockVoucherOptionDto>> FetchBranches(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<IReadOnlyList<LockVoucherOptionDto>> FetchProjectsForVoucherLock(HeadOfficeConnection office, int branchId, int lockTransId, CancellationToken cancellationToken);

        Task<LockVoucherActionResult> SaveLockVoucher(HeadOfficeConnection office, LockVoucherSaveRequest request, CancellationToken cancellationToken);

        Task<LockVoucherActionResult> DeleteLockDetails(HeadOfficeConnection office, int lockTransId, CancellationToken cancellationToken);
    }
}
