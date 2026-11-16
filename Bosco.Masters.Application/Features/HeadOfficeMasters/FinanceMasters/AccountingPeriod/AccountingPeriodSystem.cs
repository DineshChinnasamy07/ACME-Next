/*  Class Name      : AccountingPeriodSystem.cs
 *  Purpose         : Accounting Period use cases (list, get, save, delete, status)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.AccountingPeriod;

namespace Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public class AccountingPeriodSystem
    {
        private readonly IAccountingPeriodRepository _periods;

        public AccountingPeriodSystem(IAccountingPeriodRepository periods)
        {
            _periods = periods;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _periods.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<AccountingPeriodListItemDto>> FetchAccountingPeriodDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _periods.FetchAccountingPeriodDetails(office, cancellationToken);
        }

        public Task<AccountingYear?> FillAccountingPeriodDetails(HeadOfficeConnection office, int accPeriodId, CancellationToken cancellationToken)
        {
            if (accPeriodId <= 0)
            {
                return Task.FromResult<AccountingYear?>(null);
            }

            return _periods.FillAccountingPeriodDetails(office, accPeriodId, cancellationToken);
        }

        public Task<AccountingPeriodActionResult> SaveAccountingPeriodDetails(HeadOfficeConnection office, AccountingPeriodSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new AccountingPeriodActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_DETAILS_REQUIRED)
                });
            }

            return _periods.SaveAccountingPeriodDetails(office, request, cancellationToken);
        }

        public Task<AccountingPeriodActionResult> DeleteAccountingPeriodDetails(HeadOfficeConnection office, int accPeriodId, CancellationToken cancellationToken)
        {
            return _periods.DeleteAccountingPeriodDetails(office, accPeriodId, cancellationToken);
        }

        public Task<AccountingPeriodActionResult> UpdateOfficeStatus(HeadOfficeConnection office, int accPeriodId, int status, CancellationToken cancellationToken)
        {
            return _periods.UpdateOfficeStatus(office, accPeriodId, status, cancellationToken);
        }
    }
}
