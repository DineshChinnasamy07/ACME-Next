/*  Class Name      : HeadOfficeDashboardRepository.cs
 *  Purpose         : Head Office Home dashboard on the HO tenant database. Ports the legacy ACCPPORTAL home pages:
 *                    HomeLogin (Quick View: Branch / Project filter, Cash-Bank-FD closing balance, project-wise data
 *                    status, Non Conforming Branches) and HomeDashboard (Data Status: vouchers per branch per month
 *                    of the active financial year).
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using System.Data;
using System.Data.Common;
using System.Globalization;
using Acme.Common;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.Dashboard;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Dashboard;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.Dashboard
{
    public class HeadOfficeDashboardRepository : IHeadOfficeDashboardRepository
    {
        // Branch office STATUS 2 = activated.
        private const int BranchActivated = 2;

        // Fixed ledger groups (legacy FixedLedgerGroup): Bank Accounts, Cash-in-hand, Fixed Deposits.
        private const int BankGroupId = 12;
        private const int CashGroupId = 13;
        private const int FixedDepositGroupId = 14;

        // Legacy Quick View shows the last three months (two months back to the current month).
        private const int PendingMonthSpan = 3;

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public HeadOfficeDashboardRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        #region Helpers
        private sealed class BranchRow
        {
            public int Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int Status { get; set; }
            public string? Incharge { get; set; }
            public string? Email { get; set; }
            public string? Mobile { get; set; }
        }

        private sealed class CountRow
        {
            public int BranchId { get; set; }
            public int ProjectId { get; set; }
            public DateTime Month { get; set; }
            public int Count { get; set; }
        }

        private MastersDbContext OpenTenant(HeadOfficeConnection office)
        {
            string host = (office.HostName ?? string.Empty).Trim();
            string database = (office.DbName ?? string.Empty).Trim();
            string user = (office.UserName ?? string.Empty).Trim();
            if (host.Length == 0 || database.Length == 0 || user.Length == 0)
            {
                throw new InvalidOperationException(MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeDbMissingForBranch));
            }

            int port;
            if (!int.TryParse(_configuration["HeadOfficeTenant:Port"], out port) || port <= 0)
            {
                port = 3308;
            }

            string connectionString = "Host=" + host
                + ";Port=" + port.ToString(CultureInfo.InvariantCulture)
                + ";Database=" + database
                + ";Username=" + user
                + ";Password=" + (office.Password ?? string.Empty);
            DbContextOptionsBuilder<MastersDbContext> builder = new DbContextOptionsBuilder<MastersDbContext>();
            builder.UseNpgsql(connectionString);
            return new MastersDbContext(builder.Options);
        }

        private static void AddParameter(DbCommand command, string name, object value, DbType type)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            parameter.DbType = type;
            command.Parameters.Add(parameter);
        }

        private static async Task<List<T>> Query<T>(DbConnection connection, string sql, Action<DbCommand>? bind,
            Func<DbDataReader, T> map, CancellationToken cancellationToken)
        {
            List<T> rows = new List<T>();
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                bind?.Invoke(command);
                await using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        rows.Add(map(reader));
                    }
                }
            }

            return rows;
        }

        private static string? Text(DbDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                return null;
            }

            string value = Convert.ToString(reader.GetValue(index), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
            return value.Length == 0 ? null : value;
        }

        private static int Number(DbDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? 0 : Convert.ToInt32(reader.GetValue(index), CultureInfo.InvariantCulture);
        }

        private static DateTime MonthStart(DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        }

        private static string MonthKey(DateTime value)
        {
            return value.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        private static string MonthLabel(DateTime value)
        {
            return value.ToString("MMM-yyyy", CultureInfo.InvariantCulture);
        }

        private static List<DateTime> MonthsBetween(DateTime from, DateTime to)
        {
            List<DateTime> months = new List<DateTime>();
            for (DateTime month = MonthStart(from); month <= to; month = month.AddMonths(1))
            {
                months.Add(month);
            }

            return months;
        }

        private static async Task<bool> MappingTableExists(DbConnection connection, CancellationToken cancellationToken)
        {
            List<bool> found = await Query(connection, "SELECT to_regclass('public.project_branch_mapping') IS NOT NULL", null,
                reader => reader.GetBoolean(0), cancellationToken);
            return found.Count > 0 && found[0];
        }

        // Project → branches it is mapped to (legacy PROJECT_BRANCH plus the new PROJECT_BRANCH_MAPPING).
        private static async Task<Dictionary<int, HashSet<int>>> ProjectBranches(DbConnection connection, CancellationToken cancellationToken)
        {
            string sql = "SELECT \"PROJECT_ID\", \"BRANCH_ID\" FROM project_branch";
            if (await MappingTableExists(connection, cancellationToken))
            {
                sql += " UNION SELECT \"PROJECT_ID\", \"BRANCH_ID\" FROM project_branch_mapping";
            }

            Dictionary<int, HashSet<int>> map = new Dictionary<int, HashSet<int>>();
            foreach ((int projectId, int branchId) in await Query(connection, sql, null,
                         reader => (Number(reader, 0), Number(reader, 1)), cancellationToken))
            {
                if (!map.TryGetValue(projectId, out HashSet<int>? branches))
                {
                    branches = new HashSet<int>();
                    map[projectId] = branches;
                }

                branches.Add(branchId);
            }

            return map;
        }

        private static Task<List<BranchRow>> Branches(DbConnection connection, CancellationToken cancellationToken)
        {
            return Query(connection,
                "SELECT \"BRANCH_OFFICE_ID\", \"BRANCH_OFFICE_CODE\", \"BRANCH_OFFICE_NAME\", \"STATUS\", \"INCHARGE_NAME\", \"BRANCH_EMAIL_ID\", \"MOBILE_NO\" " +
                "FROM branch_office ORDER BY \"BRANCH_OFFICE_NAME\"", null,
                reader => new BranchRow
                {
                    Id = Number(reader, 0),
                    Code = Text(reader, 1) ?? string.Empty,
                    Name = Text(reader, 2) ?? string.Empty,
                    Status = Number(reader, 3),
                    Incharge = Text(reader, 4),
                    Email = Text(reader, 5),
                    Mobile = Text(reader, 6)
                }, cancellationToken);
        }

        private static async Task<Dictionary<int, string>> ProjectNames(DbConnection connection, CancellationToken cancellationToken)
        {
            List<(int Id, string Name)> rows = await Query(connection,
                "SELECT \"PROJECT_ID\", \"PROJECT\" FROM master_project WHERE COALESCE(\"DELETE_FLAG\", 0) <> 1", null,
                reader => (Number(reader, 0), Text(reader, 1) ?? string.Empty), cancellationToken);
            return rows.GroupBy(row => row.Id).ToDictionary(group => group.Key, group => group.First().Name);
        }

        private static async Task<List<DashboardYearDto>> Years(DbConnection connection, CancellationToken cancellationToken)
        {
            return await Query(connection,
                "SELECT \"ACC_YEAR_ID\", \"YEAR_FROM\", \"YEAR_TO\", \"STATUS\" FROM accounting_year ORDER BY \"YEAR_FROM\" DESC", null,
                reader => new DashboardYearDto
                {
                    Id = Number(reader, 0),
                    YearFrom = reader.GetDateTime(1).Date,
                    YearTo = reader.GetDateTime(2).Date,
                    IsActive = Number(reader, 3) == 1
                }, cancellationToken);
        }

        // Optional branch / project filter on a table alias (legacy "{ AND BRANCH_ID IN (?BRANCH_ID) }").
        private static string Scope(string alias, DashboardSummaryRequest request)
        {
            string where = string.Empty;
            if (request.BranchId > 0)
            {
                where += " AND " + alias + ".\"BRANCH_ID\" = @branchId";
            }

            if (request.ProjectId > 0)
            {
                where += " AND " + alias + ".\"PROJECT_ID\" = @projectId";
            }

            return where;
        }

        private static void BindScope(DbCommand command, DashboardSummaryRequest request)
        {
            if (request.BranchId > 0)
            {
                AddParameter(command, "branchId", request.BranchId, DbType.Int32);
            }

            if (request.ProjectId > 0)
            {
                AddParameter(command, "projectId", request.ProjectId, DbType.Int32);
            }
        }
        #endregion

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            string code = (headOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return Task.FromResult<HeadOfficeConnection?>(null);
            }

            return _portal.HeadOfficeConnections.AsNoTracking()
                .FirstOrDefaultAsync(office => office.HeadOfficeCode == code, cancellationToken);
        }

        public async Task<DashboardFiltersDto> FetchDashboardFilters(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                DbConnection connection = tenant.Database.GetDbConnection();
                await connection.OpenAsync(cancellationToken);

                List<BranchRow> branches = await Branches(connection, cancellationToken);
                Dictionary<int, HashSet<int>> projectBranches = await ProjectBranches(connection, cancellationToken);
                Dictionary<int, string> projects = await ProjectNames(connection, cancellationToken);

                return new DashboardFiltersDto
                {
                    Years = await Years(connection, cancellationToken),
                    Branches = branches.Where(row => row.Status == BranchActivated)
                        .Select(row => new DashboardOptionDto { Id = row.Id, Code = row.Code, Name = row.Name })
                        .ToList(),
                    Projects = projects.OrderBy(row => row.Value, StringComparer.OrdinalIgnoreCase)
                        .Select(row => new DashboardProjectOptionDto
                        {
                            Id = row.Key,
                            Name = row.Value,
                            BranchIds = projectBranches.TryGetValue(row.Key, out HashSet<int>? ids) ? ids.OrderBy(id => id).ToList() : new List<int>()
                        })
                        .ToList()
                };
            }
        }

        public async Task<DashboardSummaryDto> FetchDashboardSummary(HeadOfficeConnection office, DashboardSummaryRequest request, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                DbConnection connection = tenant.Database.GetDbConnection();
                await connection.OpenAsync(cancellationToken);

                // Financial year: the chosen one, else the active one (legacy FetchActiveTransactionPeriod), else the
                // latest, else April–March of today.
                List<DashboardYearDto> years = await Years(connection, cancellationToken);
                DashboardYearDto? year = years.FirstOrDefault(row => row.Id == request.YearId && request.YearId > 0)
                    ?? years.FirstOrDefault(row => row.IsActive)
                    ?? years.FirstOrDefault();

                // No year chosen and the active year has no vouchers yet (new year just opened): open on the year that
                // holds the latest voucher instead, so Home does not come up empty.
                if (request.YearId <= 0 && year != null)
                {
                    List<DateTime?> latest = await Query(connection, "SELECT MAX(\"VOUCHER_DATE\") FROM voucher_master_trans", null,
                        reader => reader.IsDBNull(0) ? (DateTime?)null : reader.GetDateTime(0).Date, cancellationToken);
                    DateTime? latestVoucher = latest.Count > 0 ? latest[0] : null;
                    if (latestVoucher.HasValue && (latestVoucher.Value < year.YearFrom || latestVoucher.Value > year.YearTo))
                    {
                        year = years.FirstOrDefault(row => row.YearFrom <= latestVoucher.Value && row.YearTo >= latestVoucher.Value) ?? year;
                    }
                }

                DateTime today = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);
                DateTime yearFrom = year != null ? year.YearFrom : new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);
                DateTime yearTo = year != null ? year.YearTo : yearFrom.AddYears(1).AddDays(-1);
                List<DateTime> months = MonthsBetween(yearFrom, yearTo);
                Dictionary<DateTime, int> monthIndex = months.Select((month, index) => (month, index)).ToDictionary(pair => pair.month, pair => pair.index);

                List<BranchRow> branches = await Branches(connection, cancellationToken);
                Dictionary<int, BranchRow> branchById = branches.ToDictionary(row => row.Id);
                Dictionary<int, HashSet<int>> projectBranches = await ProjectBranches(connection, cancellationToken);
                Dictionary<int, string> projectNames = await ProjectNames(connection, cancellationToken);

                // Vouchers per branch, project and month of the year (legacy FetchDataSynStatusProjectWise).
                List<CountRow> counts = await Query(connection,
                    "SELECT v.\"BRANCH_ID\", v.\"PROJECT_ID\", date_trunc('month', v.\"VOUCHER_DATE\") AS month, COUNT(*) " +
                    "FROM voucher_master_trans v " +
                    "WHERE v.\"VOUCHER_DATE\" >= @dateFrom AND v.\"VOUCHER_DATE\" < @dateTo" + Scope("v", request) +
                    " GROUP BY 1, 2, 3",
                    command =>
                    {
                        AddParameter(command, "dateFrom", yearFrom, DbType.DateTime2);
                        AddParameter(command, "dateTo", yearTo.AddDays(1), DbType.DateTime2);
                        BindScope(command, request);
                    },
                    reader => new CountRow
                    {
                        BranchId = Number(reader, 0),
                        ProjectId = Number(reader, 1),
                        Month = MonthStart(reader.GetDateTime(2)),
                        Count = Number(reader, 3)
                    }, cancellationToken);

                // Branches in scope: activated branches (or the chosen one), narrowed to the project's branches.
                HashSet<int> projectScope = request.ProjectId > 0 && projectBranches.TryGetValue(request.ProjectId, out HashSet<int>? mapped)
                    ? mapped : new HashSet<int>();
                List<BranchRow> scopeBranches = branches
                    .Where(row => request.BranchId > 0 ? row.Id == request.BranchId : row.Status == BranchActivated)
                    .Where(row => request.ProjectId == 0 || projectScope.Contains(row.Id) || counts.Any(count => count.BranchId == row.Id))
                    .ToList();

                DashboardSummaryDto summary = new DashboardSummaryDto
                {
                    YearId = year?.Id ?? 0,
                    YearFrom = yearFrom,
                    YearTo = yearTo,
                    Months = months.Select(MonthKey).ToList(),
                    RowKind = request.BranchId > 0 ? "project" : "branch"
                };

                // Data status rows: per branch (all branches) or per project of the chosen branch.
                if (request.BranchId == 0)
                {
                    HashSet<int> rowIds = new HashSet<int>(scopeBranches.Select(row => row.Id));
                    rowIds.UnionWith(counts.Select(count => count.BranchId));
                    foreach (int branchId in rowIds)
                    {
                        BranchRow? branch;
                        branchById.TryGetValue(branchId, out branch);
                        summary.Rows.Add(BuildRow(branchId, branch?.Code ?? string.Empty, branch?.Name ?? ("Branch " + branchId),
                            counts.Where(count => count.BranchId == branchId), monthIndex, months.Count));
                    }
                }
                else
                {
                    HashSet<int> rowIds = new HashSet<int>(projectBranches
                        .Where(pair => pair.Value.Contains(request.BranchId) && projectNames.ContainsKey(pair.Key))
                        .Select(pair => pair.Key));
                    if (request.ProjectId > 0)
                    {
                        rowIds.RemoveWhere(id => id != request.ProjectId);
                        rowIds.Add(request.ProjectId);
                    }

                    rowIds.UnionWith(counts.Select(count => count.ProjectId));
                    foreach (int projectId in rowIds)
                    {
                        summary.Rows.Add(BuildRow(projectId, string.Empty,
                            projectNames.TryGetValue(projectId, out string? name) ? name : ("Project " + projectId),
                            counts.Where(count => count.ProjectId == projectId), monthIndex, months.Count));
                    }
                }

                summary.Rows = summary.Rows.OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase).ToList();
                summary.MonthTotals = Enumerable.Range(0, months.Count).Select(index => summary.Rows.Sum(row => row.Counts[index])).ToList();
                summary.TotalVouchers = summary.MonthTotals.Sum();
                summary.Branches = scopeBranches.Count;
                summary.BranchesWithData = scopeBranches.Count(row => counts.Any(count => count.BranchId == row.Id));
                summary.Projects = request.ProjectId > 0 ? 1 : (request.BranchId > 0
                    ? summary.Rows.Count
                    : projectNames.Keys.Count(id => projectBranches.ContainsKey(id)));

                // Balance as on the latest voucher date of the year (legacy FetchRecentVoucherDate + ucAccountBalance).
                List<DateTime?> last = await Query(connection,
                    "SELECT MAX(v.\"VOUCHER_DATE\") FROM voucher_master_trans v " +
                    "WHERE v.\"VOUCHER_DATE\" >= @dateFrom AND v.\"VOUCHER_DATE\" < @dateTo" + Scope("v", request),
                    command =>
                    {
                        AddParameter(command, "dateFrom", yearFrom, DbType.DateTime2);
                        AddParameter(command, "dateTo", yearTo.AddDays(1), DbType.DateTime2);
                        BindScope(command, request);
                    },
                    reader => reader.IsDBNull(0) ? (DateTime?)null : reader.GetDateTime(0).Date, cancellationToken);
                summary.LastVoucherDate = last.Count > 0 ? last[0] : null;
                summary.BalanceAsOn = summary.LastVoucherDate ?? (today < yearTo ? (today < yearFrom ? yearFrom : today) : yearTo);
                summary.Balances = await FetchBalances(connection, request, summary.BalanceAsOn, cancellationToken);

                // Non Conforming Branches: branches with mapped projects that have months without any voucher in the
                // pending period (default: last three months of the year up to today).
                DateTime lastMonth = MonthStart(today < yearTo ? (today < yearFrom ? yearFrom : today) : yearTo);
                DateTime pendingFrom = MonthStart(request.PendingFrom ?? lastMonth.AddMonths(1 - PendingMonthSpan));
                DateTime pendingTo = MonthStart(request.PendingTo ?? lastMonth);
                if (pendingFrom < MonthStart(yearFrom)) pendingFrom = MonthStart(yearFrom);
                if (pendingTo > MonthStart(yearTo)) pendingTo = MonthStart(yearTo);
                if (pendingTo < pendingFrom) pendingTo = pendingFrom;
                summary.PendingFrom = pendingFrom;
                summary.PendingTo = pendingTo.AddMonths(1).AddDays(-1);

                HashSet<int> mappedBranches = new HashSet<int>(projectBranches
                    .Where(pair => request.ProjectId == 0 || pair.Key == request.ProjectId)
                    .SelectMany(pair => pair.Value));
                List<DateTime> pendingMonths = MonthsBetween(pendingFrom, pendingTo);
                foreach (BranchRow branch in scopeBranches.Where(row => mappedBranches.Contains(row.Id)))
                {
                    HashSet<DateTime> active = new HashSet<DateTime>(counts.Where(count => count.BranchId == branch.Id && count.Count > 0).Select(count => count.Month));
                    List<string> missing = pendingMonths.Where(month => !active.Contains(month)).Select(MonthLabel).ToList();
                    if (missing.Count > 0)
                    {
                        summary.NonConforming.Add(new DashboardNonConformingDto
                        {
                            BranchId = branch.Id,
                            Code = branch.Code,
                            Name = branch.Name,
                            Incharge = branch.Incharge,
                            Email = branch.Email,
                            Mobile = branch.Mobile,
                            PendingMonths = missing
                        });
                    }
                }

                return summary;
            }
        }

        private static DashboardStatusRowDto BuildRow(int id, string code, string name, IEnumerable<CountRow> counts,
            Dictionary<DateTime, int> monthIndex, int monthCount)
        {
            DashboardStatusRowDto row = new DashboardStatusRowDto { Id = id, Code = code, Name = name, Counts = new List<int>(new int[monthCount]) };
            foreach (CountRow count in counts)
            {
                if (monthIndex.TryGetValue(count.Month, out int index))
                {
                    row.Counts[index] += count.Count;
                }
            }

            row.Total = row.Counts.Sum();
            return row;
        }

        // Closing balance of the Cash, Bank and FD ledgers: latest LEDGER_BALANCE row on or before the date for every
        // branch / project / ledger, summed DR minus CR (legacy TransBalance.FetchBalance).
        private static async Task<List<DashboardBalanceDto>> FetchBalances(DbConnection connection, DashboardSummaryRequest request,
            DateTime asOn, CancellationToken cancellationToken)
        {
            var ledgers = await Query(connection,
                "SELECT ml.\"GROUP_ID\", ml.\"LEDGER_ID\", ml.\"LEDGER_NAME\", " +
                "SUM(CASE WHEN lb.\"TRANS_MODE\" = 'DR' THEN lb.\"AMOUNT\" ELSE -lb.\"AMOUNT\" END) AS amount " +
                "FROM (SELECT DISTINCT ON (l.\"BRANCH_ID\", l.\"PROJECT_ID\", l.\"LEDGER_ID\") l.\"LEDGER_ID\", l.\"AMOUNT\", l.\"TRANS_MODE\" " +
                "      FROM ledger_balance l " +
                "      WHERE l.\"BALANCE_DATE\" <= @asOn" + Scope("l", request) +
                "        AND l.\"LEDGER_ID\" IN (SELECT \"LEDGER_ID\" FROM master_ledger WHERE \"GROUP_ID\" IN (@bank, @cash, @fd)) " +
                "      ORDER BY l.\"BRANCH_ID\", l.\"PROJECT_ID\", l.\"LEDGER_ID\", l.\"BALANCE_DATE\" DESC) lb " +
                "JOIN master_ledger ml ON ml.\"LEDGER_ID\" = lb.\"LEDGER_ID\" " +
                "GROUP BY ml.\"GROUP_ID\", ml.\"LEDGER_ID\", ml.\"LEDGER_NAME\"",
                command =>
                {
                    AddParameter(command, "asOn", asOn, DbType.DateTime2);
                    AddParameter(command, "bank", BankGroupId, DbType.Int32);
                    AddParameter(command, "cash", CashGroupId, DbType.Int32);
                    AddParameter(command, "fd", FixedDepositGroupId, DbType.Int32);
                    BindScope(command, request);
                },
                reader => (GroupId: Number(reader, 0), LedgerId: Number(reader, 1), Name: Text(reader, 2) ?? string.Empty,
                    Amount: reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3), CultureInfo.InvariantCulture)),
                cancellationToken);

            (int Id, string Name)[] groups = { (CashGroupId, "Cash"), (BankGroupId, "Bank"), (FixedDepositGroupId, "Fixed Deposit") };
            return groups.Select(group =>
            {
                var rows = ledgers.Where(row => row.GroupId == group.Id && row.Amount != 0m).OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase).ToList();
                decimal net = rows.Sum(row => row.Amount);
                return new DashboardBalanceDto
                {
                    GroupId = group.Id,
                    Group = group.Name,
                    Amount = Math.Abs(net),
                    Mode = net >= 0 ? "DR" : "CR",
                    Ledgers = rows.Select(row => new DashboardBalanceLedgerDto
                    {
                        LedgerId = row.LedgerId,
                        Name = row.Name,
                        Amount = Math.Abs(row.Amount),
                        Mode = row.Amount >= 0 ? "DR" : "CR"
                    }).ToList()
                };
            }).ToList();
        }
    }
}
