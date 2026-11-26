using Acme.Common;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class GeneralateLedgerGroupRepository : IGeneralateLedgerGroupRepository
    {
        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public GeneralateLedgerGroupRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static GeneralateLedgerActionResult Ok(string message, int id)
        {
            return new GeneralateLedgerActionResult { Success = true, Message = message, ConLedgerId = id };
        }

        private static GeneralateLedgerActionResult Fail(string message)
        {
            return new GeneralateLedgerActionResult { Success = false, Message = message };
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
                + ";Port=" + port.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ";Database=" + database
                + ";Username=" + user
                + ";Password=" + (office.Password ?? string.Empty);
            DbContextOptionsBuilder<MastersDbContext> builder = new DbContextOptionsBuilder<MastersDbContext>();
            builder.UseNpgsql(connectionString);
            return new MastersDbContext(builder.Options);
        }

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

        public async Task<IReadOnlyList<GeneralateLedgerListItemDto>> FetchAllGeneralateLedgers(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<CongregationLedger> rows = await tenant.CongregationLedgers.AsNoTracking()
                    .OrderBy(row => row.ConLedgerCode)
                    .ThenBy(row => row.ConLedgerName)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> names = rows.ToDictionary(row => row.ConLedgerId, row => row.ConLedgerName);
                HashSet<int> parentsWithChildren = new HashSet<int>(
                    rows.Where(row => row.ConParentLedgerId != row.ConLedgerId && row.ConParentLedgerId > 0)
                        .Select(row => row.ConParentLedgerId));

                List<GeneralateLedgerListItemDto> items = new List<GeneralateLedgerListItemDto>(rows.Count);
                for (int i = 0; i < rows.Count; i++)
                {
                    CongregationLedger row = rows[i];
                    string parentName = string.Empty;
                    if (row.ConParentLedgerId > 0 && row.ConParentLedgerId != row.ConLedgerId)
                    {
                        names.TryGetValue(row.ConParentLedgerId, out parentName!);
                        parentName = parentName ?? string.Empty;
                    }

                    items.Add(new GeneralateLedgerListItemDto
                    {
                        ConLedgerId = row.ConLedgerId,
                        ConLedgerCode = row.ConLedgerCode ?? string.Empty,
                        ConLedgerName = row.ConLedgerName ?? string.Empty,
                        ConParentLedgerId = row.ConParentLedgerId,
                        ParentName = parentName,
                        ConMainParentId = row.ConMainParentId,
                        HasChildren = parentsWithChildren.Contains(row.ConLedgerId)
                    });
                }

                return items;
            }
        }

        public async Task<IReadOnlyList<GeneralateLedgerOptionDto>> FetchAllGeneralateParentLedgers(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.CongregationLedgers.AsNoTracking()
                    .OrderBy(row => row.ConLedgerCode)
                    .ThenBy(row => row.ConLedgerName)
                    .Select(row => new GeneralateLedgerOptionDto
                    {
                        Id = row.ConLedgerId,
                        Name = ((row.ConLedgerCode ?? string.Empty) + "  " + (row.ConLedgerName ?? string.Empty)).Trim(),
                        MainParentId = row.ConMainParentId
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<CongregationLedger?> FetchGeneralateLedgerById(HeadOfficeConnection office, int conLedgerId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.CongregationLedgers.AsNoTracking()
                    .FirstOrDefaultAsync(row => row.ConLedgerId == conLedgerId, cancellationToken);
            }
        }

        public async Task<GeneralateLedgerActionResult> SaveGeneralateLedger(HeadOfficeConnection office, GeneralateLedgerSaveRequest request, CancellationToken cancellationToken)
        {
            string code = (request.ConLedgerCode ?? string.Empty).Trim().ToUpperInvariant();
            string name = (request.ConLedgerName ?? string.Empty).Trim();
            if (code.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_CODE_EMPTY));
            }

            if (code.Length > 10)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_CODE_MAX));
            }

            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_NAME_EMPTY));
            }

            if (name.Length > 100)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_NAME_MAX));
            }

            if (request.ConLedgerId <= 0 && request.ConParentLedgerId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_PARENT_EMPTY));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    string lowerCode = code.ToLowerInvariant();
                    string lowerName = name.ToLowerInvariant();
                    bool duplicate = await tenant.CongregationLedgers.AnyAsync(
                        row => row.ConLedgerCode.ToLower() == lowerCode
                            && row.ConLedgerName.ToLower() == lowerName
                            && row.ConLedgerId != request.ConLedgerId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_EXISTS));
                    }

                    int mainParentId = 0;
                    int parentId = request.ConParentLedgerId;
                    if (parentId > 0)
                    {
                        CongregationLedger? parent = await tenant.CongregationLedgers.AsNoTracking()
                            .FirstOrDefaultAsync(row => row.ConLedgerId == parentId, cancellationToken);
                        if (parent == null)
                        {
                            return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_PARENT_EMPTY));
                        }

                        mainParentId = parent.ConMainParentId > 0 ? parent.ConMainParentId : parent.ConLedgerId;
                    }

                    if (request.ConLedgerId <= 0)
                    {
                        CongregationLedger row = new CongregationLedger
                        {
                            ConLedgerCode = code,
                            ConLedgerName = name,
                            ConParentLedgerId = parentId,
                            ConMainParentId = mainParentId
                        };
                        tenant.CongregationLedgers.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);

                        if (parentId == 0)
                        {
                            row.ConParentLedgerId = row.ConLedgerId;
                            row.ConMainParentId = row.ConLedgerId;
                            await tenant.SaveChangesAsync(cancellationToken);
                        }

                        return Ok(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_SAVE_SUCCESS), row.ConLedgerId);
                    }

                    CongregationLedger? existing = await tenant.CongregationLedgers.FirstOrDefaultAsync(
                        row => row.ConLedgerId == request.ConLedgerId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_NOT_FOUND));
                    }

                    existing.ConLedgerCode = code;
                    existing.ConLedgerName = name;
                    if (parentId > 0)
                    {
                        existing.ConParentLedgerId = parentId;
                        existing.ConMainParentId = mainParentId;
                    }

                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_SAVE_SUCCESS), existing.ConLedgerId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<GeneralateLedgerActionResult> DeleteLedger(HeadOfficeConnection office, int conLedgerId, CancellationToken cancellationToken)
        {
            if (conLedgerId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    CongregationLedger? existing = await tenant.CongregationLedgers.FirstOrDefaultAsync(
                        row => row.ConLedgerId == conLedgerId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_NOT_FOUND));
                    }

                    bool hasChildren = await tenant.CongregationLedgers.AnyAsync(
                        row => row.ConParentLedgerId == conLedgerId && row.ConLedgerId != conLedgerId,
                        cancellationToken);
                    if (hasChildren)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_CAN_DELETE));
                    }

                    List<CongregationLedgerMap> maps = await tenant.CongregationLedgerMaps
                        .Where(row => row.ConLedgerId == conLedgerId)
                        .ToListAsync(cancellationToken);
                    if (maps.Count > 0)
                    {
                        tenant.CongregationLedgerMaps.RemoveRange(maps);
                    }

                    tenant.CongregationLedgers.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_DELETE_SUCCESS), conLedgerId);
                }
            }
            catch (Exception)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_CAN_DELETE));
            }
        }
    }
}
