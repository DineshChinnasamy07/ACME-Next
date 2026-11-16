/*  Class Name      : AccountingPeriodRepository.cs
 *  Purpose         : ACCOUNTING_YEAR list, get, save, delete, status in the HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public class AccountingPeriodRepository : IAccountingPeriodRepository
    {
        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public AccountingPeriodRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        #region Helpers
        private static AccountingPeriodActionResult Ok(string message, int accYearId)
        {
            return new AccountingPeriodActionResult { Success = true, Message = message, AccYearId = accYearId };
        }

        private static AccountingPeriodActionResult Fail(string message)
        {
            return new AccountingPeriodActionResult { Success = false, Message = message };
        }

        private static string YearStatusText(int status)
        {
            return status == 0 ? "Inactive" : "Active";
        }

        private static AccountingPeriodListItemDto ToItem(AccountingYear row)
        {
            return new AccountingPeriodListItemDto
            {
                AccYearId = row.AccYearId,
                YearFrom = row.YearFrom,
                YearTo = row.YearTo,
                Status = row.Status,
                YearStatus = YearStatusText(row.Status)
            };
        }

        private MastersDbContext OpenTenant(HeadOfficeConnection office)
        {
            string host = (office.HostName ?? string.Empty).Trim();
            string database = (office.DbName ?? string.Empty).Trim();
            string user = (office.UserName ?? string.Empty).Trim();
            if (host.Length == 0 || database.Length == 0 || user.Length == 0)
            {
                throw new InvalidOperationException(
                    MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeDbMissingForBranch));
            }

            int port;
            if (!int.TryParse(_configuration["HeadOfficeTenant:Port"], out port) || port <= 0)
            {
                port = 3308;
            }

            string connectionString = "Host=" + host
                + ";Port=" + port.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ";Database=" + database
                + ";Username=" + user
                + ";Password=" + (office.Password ?? string.Empty);

            DbContextOptionsBuilder<MastersDbContext> builder = new DbContextOptionsBuilder<MastersDbContext>();
            builder.UseNpgsql(connectionString);
            return new MastersDbContext(builder.Options);
        }

        private static async Task<bool> HasLedgerBalanceInPeriod(
            MastersDbContext tenant,
            DateTime yearFrom,
            DateTime yearTo,
            CancellationToken cancellationToken)
        {
            List<int> rows = await tenant.Database
                .SqlQueryRaw<int>(
                    "SELECT 1 AS \"Value\" FROM public.ledger_balance WHERE \"BALANCE_DATE\" BETWEEN {0} AND {1} LIMIT 1",
                    yearFrom,
                    yearTo)
                .ToListAsync(cancellationToken);
            return rows.Count > 0;
        }
        #endregion

        #region Implementation of IAccountingPeriodRepository
        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            string code = (headOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return Task.FromResult<HeadOfficeConnection?>(null);
            }

            return _portal.HeadOfficeConnections
                .AsNoTracking()
                .FirstOrDefaultAsync(office => office.HeadOfficeCode == code, cancellationToken);
        }

        public async Task<IReadOnlyList<AccountingPeriodListItemDto>> FetchAccountingPeriodDetails(
            HeadOfficeConnection office,
            CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<AccountingYear> rows = await tenant.AccountingYears
                    .AsNoTracking()
                    .OrderBy(row => row.YearFrom)
                    .ToListAsync(cancellationToken);
                return rows.Select(ToItem).ToList();
            }
        }

        public async Task<AccountingYear?> FillAccountingPeriodDetails(
            HeadOfficeConnection office,
            int accPeriodId,
            CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.AccountingYears
                    .AsNoTracking()
                    .FirstOrDefaultAsync(row => row.AccYearId == accPeriodId, cancellationToken);
            }
        }

        public async Task<AccountingPeriodActionResult> SaveAccountingPeriodDetails(
            HeadOfficeConnection office,
            AccountingPeriodSaveRequest request,
            CancellationToken cancellationToken)
        {
            if (request.YearFrom == default(DateTime))
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_YEAR_FROM_EMPTY));
            }

            if (request.YearTo == default(DateTime))
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_YEAR_TO_EMPTY));
            }

            if (request.YearFrom > request.YearTo)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_YEAR_EQUAL_EMPTY));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    bool exists = await tenant.AccountingYears.AnyAsync(
                        row => row.YearFrom >= request.YearFrom
                            && row.YearTo <= request.YearTo
                            && row.AccYearId != request.AccPeriodId,
                        cancellationToken);
                    if (exists)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_EXISTS));
                    }

                    if (request.AccPeriodId <= 0)
                    {
                        int nextId = await tenant.AccountingYears.MaxAsync(
                            row => (int?)row.AccYearId,
                            cancellationToken) ?? 0;
                        nextId++;

                        bool isFirst = !await tenant.AccountingYears.AnyAsync(cancellationToken);
                        AccountingYear row = new AccountingYear
                        {
                            AccYearId = nextId,
                            YearFrom = request.YearFrom.Date,
                            YearTo = request.YearTo.Date,
                            Status = isFirst ? 1 : 0,
                            IsFirstAccountingYear = isFirst ? 1 : 0,
                            BooksBeginningFrom = isFirst ? request.YearFrom.Date : (DateTime?)null
                        };
                        tenant.AccountingYears.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(
                            MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_SAVE_SUCCESS),
                            row.AccYearId);
                    }

                    AccountingYear? existing = await tenant.AccountingYears.FirstOrDefaultAsync(
                        row => row.AccYearId == request.AccPeriodId,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_NOT_FOUND));
                    }

                    existing.YearFrom = request.YearFrom.Date;
                    existing.YearTo = request.YearTo.Date;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(
                        MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_SAVE_SUCCESS),
                        existing.AccYearId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<AccountingPeriodActionResult> DeleteAccountingPeriodDetails(
            HeadOfficeConnection office,
            int accPeriodId,
            CancellationToken cancellationToken)
        {
            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    AccountingYear? existing = await tenant.AccountingYears.FirstOrDefaultAsync(
                        row => row.AccYearId == accPeriodId,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_NOT_FOUND));
                    }

                    if (existing.Status == 1)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_ACTIVE));
                    }

                    if (await HasLedgerBalanceInPeriod(tenant, existing.YearFrom, existing.YearTo, cancellationToken))
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_CANNOT_DELETE));
                    }

                    tenant.AccountingYears.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(
                        MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_DELETE_SUCCESS),
                        accPeriodId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<AccountingPeriodActionResult> UpdateOfficeStatus(
            HeadOfficeConnection office,
            int accPeriodId,
            int status,
            CancellationToken cancellationToken)
        {
            if (status != 1)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_ONE_ACTIVE));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    AccountingYear? existing = await tenant.AccountingYears.FirstOrDefaultAsync(
                        row => row.AccYearId == accPeriodId,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_NOT_FOUND));
                    }

                    if (existing.Status == 1)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_ONE_ACTIVE));
                    }

                    List<AccountingYear> rows = await tenant.AccountingYears.ToListAsync(cancellationToken);
                    for (int i = 0; i < rows.Count; i++)
                    {
                        rows[i].Status = rows[i].AccYearId == accPeriodId ? 1 : 0;
                    }

                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(
                        MessageRender.GetMessage(MessageCatalog.Master.AccountingPeriod.ACCOUNTING_PERIOD_CHANGE_PERIOD),
                        accPeriodId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
        #endregion
    }
}
