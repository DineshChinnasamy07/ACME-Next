using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher;

namespace Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class LockVoucherSystem
    {
        private readonly ILockVoucherRepository _locks;

        public LockVoucherSystem(ILockVoucherRepository locks)
        {
            _locks = locks;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _locks.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<LockVoucherListItemDto>> FetchLockedProjects(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _locks.FetchLockedProjects(office, cancellationToken);
        }

        public Task<MasterLockTrans?> FillLockVoucherDetails(HeadOfficeConnection office, int lockTransId, CancellationToken cancellationToken)
        {
            if (lockTransId <= 0)
            {
                return Task.FromResult<MasterLockTrans?>(null);
            }

            return _locks.FillLockVoucherDetails(office, lockTransId, cancellationToken);
        }

        public Task<IReadOnlyList<LockVoucherOptionDto>> FetchBranches(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _locks.FetchBranches(office, cancellationToken);
        }

        public Task<IReadOnlyList<LockVoucherOptionDto>> FetchProjectsForVoucherLock(HeadOfficeConnection office, int branchId, int lockTransId, CancellationToken cancellationToken)
        {
            return _locks.FetchProjectsForVoucherLock(office, branchId, lockTransId, cancellationToken);
        }

        public Task<LockVoucherActionResult> SaveLockVoucher(HeadOfficeConnection office, LockVoucherSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new LockVoucherActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_DETAILS_REQUIRED)
                });
            }

            return _locks.SaveLockVoucher(office, request, cancellationToken);
        }

        public Task<LockVoucherActionResult> DeleteLockDetails(HeadOfficeConnection office, int lockTransId, CancellationToken cancellationToken)
        {
            return _locks.DeleteLockDetails(office, lockTransId, cancellationToken);
        }
    }
}
