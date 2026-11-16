/*  Class Name      : IAccountingPeriodRepository.cs
 *  Purpose         : HO tenant ACCOUNTING_YEAR access for AccountingPeriodSystem
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.AccountingPeriod;

namespace Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public interface IAccountingPeriodRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<AccountingPeriodListItemDto>> FetchAccountingPeriodDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<AccountingYear?> FillAccountingPeriodDetails(HeadOfficeConnection office, int accPeriodId, CancellationToken cancellationToken);

        Task<AccountingPeriodActionResult> SaveAccountingPeriodDetails(HeadOfficeConnection office, AccountingPeriodSaveRequest request, CancellationToken cancellationToken);

        Task<AccountingPeriodActionResult> DeleteAccountingPeriodDetails(HeadOfficeConnection office, int accPeriodId, CancellationToken cancellationToken);

        Task<AccountingPeriodActionResult> UpdateOfficeStatus(HeadOfficeConnection office, int accPeriodId, int status, CancellationToken cancellationToken);
    }
}
