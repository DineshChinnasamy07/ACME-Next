/*  Class Name      : LedgerRepository.cs
 *  Purpose         : MASTER_LEDGER list, get, group lookup, save, delete in HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Ledger;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Ledger;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Bosco.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.Ledger
{
    public class LedgerRepository : ILedgerRepository
    {
        private const int FixedDepositGroupId = 14;
        private const int BankAccountsGroupId = 12;
        private const int CashInHandGroupId = 13;

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public LedgerRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        #region Helpers
        private static LedgerActionResult Ok(string message, int ledgerId)
        {
            return new LedgerActionResult { Success = true, Message = message, LedgerId = ledgerId };
        }

        private static LedgerActionResult Fail(string message)
        {
            return new LedgerActionResult { Success = false, Message = message };
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

        private static string ResolveGroupDisplay(MasterLedger ledger, MasterLedgerGroup? group)
        {
            if (string.Equals(ledger.LedgerSubType, "BK", StringComparison.OrdinalIgnoreCase))
            {
                return "Bank Accounts";
            }

            if (string.Equals(ledger.LedgerSubType, "FD", StringComparison.OrdinalIgnoreCase))
            {
                return "Fixed Deposits";
            }

            return group == null ? string.Empty : (group.LedgerGroup ?? string.Empty);
        }

        private static string NormalizeLedgerType(string? ledgerType)
        {
            string value = (ledgerType ?? string.Empty).Trim().ToUpperInvariant();
            if (value == "IK")
            {
                return "IK";
            }

            return "GN";
        }

        private static string ResolveLedgerSubType(int groupId)
        {
            return groupId == FixedDepositGroupId ? "FD" : "GN";
        }
        #endregion

        #region Implementation of ILedgerRepository
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

        public async Task<IReadOnlyList<LedgerListItemDto>> FetchLedgerDetails(
            HeadOfficeConnection office,
            CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterLedger> ledgers = await tenant.Ledgers
                    .AsNoTracking()
                    .Where(row => row.Status == 0 && row.IsBranchLedger == 0)
                    .OrderByDescending(row => row.AccessFlag)
                    .ThenBy(row => row.LedgerName)
                    .ToListAsync(cancellationToken);

                Dictionary<int, MasterLedgerGroup> groups = await tenant.LedgerGroups
                    .AsNoTracking()
                    .ToDictionaryAsync(row => row.GroupId, cancellationToken);

                List<LedgerListItemDto> items = new List<LedgerListItemDto>(ledgers.Count);
                for (int i = 0; i < ledgers.Count; i++)
                {
                    MasterLedger ledger = ledgers[i];
                    MasterLedgerGroup? group;
                    groups.TryGetValue(ledger.GroupId, out group);

                    items.Add(new LedgerListItemDto
                    {
                        LedgerId = ledger.LedgerId,
                        GroupId = ledger.GroupId,
                        LedgerCode = ledger.LedgerCode ?? string.Empty,
                        LedgerName = ledger.LedgerName ?? string.Empty,
                        Group = ResolveGroupDisplay(ledger, group),
                        LedgerType = ledger.LedgerType ?? string.Empty,
                        LedgerSubType = ledger.LedgerSubType ?? string.Empty,
                        BankAccountId = ledger.BankAccountId,
                        NatureId = group == null ? 0 : group.NatureId,
                        AccessFlag = ledger.AccessFlag,
                        BudgetGroupId = ledger.BudgetGroupId,
                        BudgetSubGroupId = ledger.BudgetSubGroupId
                    });
                }

                return items;
            }
        }

        public async Task<MasterLedger?> FillLedgerDetails(
            HeadOfficeConnection office,
            int ledgerId,
            CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.Ledgers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(row => row.LedgerId == ledgerId && row.Status == 0, cancellationToken);
            }
        }

        public async Task<IReadOnlyList<LedgerGroupLookupDto>> LoadLedgerGroupforLedgerLookup(
            HeadOfficeConnection office,
            CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                // Old FetchforLedgerLookup excludes natures 1-4 roots and Bank(12).
                return await tenant.LedgerGroups
                    .AsNoTracking()
                    .Where(row => row.GroupId != 1
                        && row.GroupId != 2
                        && row.GroupId != 3
                        && row.GroupId != 4
                        && row.GroupId != 12)
                    .OrderBy(row => row.LedgerGroup)
                    .Select(row => new LedgerGroupLookupDto
                    {
                        GroupId = row.GroupId,
                        NatureId = row.NatureId,
                        LedgerGroup = row.LedgerGroup
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<LedgerActionResult> SaveLedgerDetails(
            HeadOfficeConnection office,
            LedgerSaveRequest request,
            CancellationToken cancellationToken)
        {
            string code = (request.LedgerCode ?? string.Empty).Trim();
            string name = (request.LedgerName ?? string.Empty).Trim();

            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_NAME_EMPTY));
            }

            if (name.Length > 100)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_NAME_MAX));
            }

            if (code.Length > 15)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_CODE_MAX));
            }

            if (request.GroupId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_GROUP_EMPTY));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    bool groupOk = await tenant.LedgerGroups.AnyAsync(
                        row => row.GroupId == request.GroupId, cancellationToken);
                    if (!groupOk)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_GROUP_EMPTY));
                    }

                    string lower = name.ToLowerInvariant();
                    bool duplicate = await tenant.Ledgers.AnyAsync(
                        row => row.LedgerName.ToLower() == lower
                            && row.LedgerId != request.LedgerId
                            && row.Status == 0,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_EXISTS));
                    }

                    string ledgerType = NormalizeLedgerType(request.LedgerType);
                    string ledgerSubType = ResolveLedgerSubType(request.GroupId);
                    int fdInvTypeId = request.GroupId == FixedDepositGroupId ? request.FDInvTypeId : 0;
                    DateTime? closedOn = request.LedgerClosedOn;
                    if (closedOn.HasValue && closedOn.Value == DateTime.MinValue)
                    {
                        closedOn = null;
                    }

                    if (request.LedgerId <= 0)
                    {
                        MasterLedger row = new MasterLedger
                        {
                            LedgerCode = code.Length == 0 ? null : code,
                            LedgerName = name,
                            GroupId = request.GroupId,
                            LedgerType = ledgerType,
                            LedgerSubType = ledgerSubType,
                            BankAccountId = request.BankAccountId,
                            IsCostCenter = request.IsCostCentre,
                            IsTdsLedger = request.IsTDSApplicable,
                            Notes = request.LedgerNotes,
                            IsBankInterestLedger = request.IsFDInterestLedger,
                            IsBankFdPenaltyLedger = request.IsFDPenaltyLedger,
                            IsBankSbInterestLedger = request.IsSBInterestLedger,
                            IsBankCommissionLedger = request.IsBankCommissionLedger,
                            IsInkindLedger = request.IsInKindLedger,
                            IsDepreciationLedger = request.IsDepriciationLedger,
                            IsAssetGainLedger = request.IsAssetGainLedger,
                            IsAssetLossLedger = request.IsAssetLossLedger,
                            IsDisposalLedger = request.IsAssetDisposalLedger,
                            BudgetGroupId = request.BudgetGroupId,
                            BudgetSubGroupId = request.BudgetSubGroupId,
                            FdInvestmentTypeId = fdInvTypeId,
                            SortId = 255,
                            Status = 0,
                            AccessFlag = (int)AccessFlag.Accessable,
                            IsBranchLedger = 0,
                            DateClosed = closedOn,
                            ClosedBy = closedOn.HasValue ? 1 : 0
                        };
                        tenant.Ledgers.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_SUCCESS), row.LedgerId);
                    }

                    MasterLedger? existing = await tenant.Ledgers.FirstOrDefaultAsync(
                        row => row.LedgerId == request.LedgerId && row.Status == 0,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_NOT_FOUND));
                    }

                    if (existing.AccessFlag == (int)AccessFlag.Readonly)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.FIXED_LEDGER_EDIT));
                    }

                    existing.LedgerCode = code.Length == 0 ? null : code;
                    existing.LedgerName = name;
                    existing.GroupId = request.GroupId;
                    existing.LedgerType = ledgerType;
                    existing.LedgerSubType = ledgerSubType;
                    existing.BankAccountId = request.BankAccountId;
                    existing.IsCostCenter = request.IsCostCentre;
                    existing.IsTdsLedger = request.IsTDSApplicable;
                    existing.Notes = request.LedgerNotes;
                    existing.IsBankInterestLedger = request.IsFDInterestLedger;
                    existing.IsBankFdPenaltyLedger = request.IsFDPenaltyLedger;
                    existing.IsBankSbInterestLedger = request.IsSBInterestLedger;
                    existing.IsBankCommissionLedger = request.IsBankCommissionLedger;
                    existing.IsInkindLedger = request.IsInKindLedger;
                    existing.IsDepreciationLedger = request.IsDepriciationLedger;
                    existing.IsAssetGainLedger = request.IsAssetGainLedger;
                    existing.IsAssetLossLedger = request.IsAssetLossLedger;
                    existing.IsDisposalLedger = request.IsAssetDisposalLedger;
                    existing.BudgetGroupId = request.BudgetGroupId;
                    existing.BudgetSubGroupId = request.BudgetSubGroupId;
                    existing.FdInvestmentTypeId = fdInvTypeId;
                    existing.DateClosed = closedOn;
                    existing.ClosedBy = closedOn.HasValue ? 1 : 0;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_SUCCESS), existing.LedgerId);
                }
            }
            catch (DbUpdateException)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_EXISTS));
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<LedgerActionResult> DeleteLedgerDetails(
            HeadOfficeConnection office,
            int ledgerId,
            CancellationToken cancellationToken)
        {
            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterLedger? existing = await tenant.Ledgers.FirstOrDefaultAsync(
                        row => row.LedgerId == ledgerId && row.Status == 0,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_NOT_FOUND));
                    }

                    if (existing.AccessFlag == (int)AccessFlag.Readonly
                        || existing.GroupId == BankAccountsGroupId
                        || existing.GroupId == CashInHandGroupId
                        || string.Equals(existing.LedgerName, "Cash", StringComparison.OrdinalIgnoreCase))
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.FIXED_LEDGER_DELETE));
                    }

                    List<int> balanceRows = await tenant.Database
                        .SqlQueryRaw<int>(
                            "SELECT 1 AS \"Value\" FROM public.ledger_balance WHERE \"LEDGER_ID\" = {0} LIMIT 1",
                            ledgerId)
                        .ToListAsync(cancellationToken);
                    if (balanceRows.Count > 0)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_TRANSACTION_MADE));
                    }

                    tenant.Ledgers.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_DELETED), ledgerId);
                }
            }
            catch (DbUpdateException)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_TRANSACTION_MADE));
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
        #endregion
    }
}
