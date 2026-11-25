/*  Class Name      : LedgerGroupRepository.cs
 *  Purpose         : MASTER_LEDGER_GROUP list, lookup, get, ledgers, save, delete in HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Common;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class LedgerGroupRepository : ILedgerGroupRepository
    {
        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public LedgerGroupRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        #region Helpers
        private static LedgerGroupActionResult Ok(string message, int groupId)
        {
            return new LedgerGroupActionResult { Success = true, Message = message, GroupId = groupId };
        }

        private static LedgerGroupActionResult Fail(string message)
        {
            return new LedgerGroupActionResult { Success = false, Message = message };
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

        private static LedgerGroupListItemDto ToItem(MasterLedgerGroup row, Dictionary<int, string> names)
        {
            string parent;
            if (!names.TryGetValue(row.ParentGroupId, out parent!))
            {
                parent = string.Empty;
            }

            return new LedgerGroupListItemDto
            {
                GroupId = row.GroupId,
                GroupCode = row.GroupCode ?? string.Empty,
                LedgerGroup = row.LedgerGroup ?? string.Empty,
                ParentGroupId = row.ParentGroupId,
                ParentGroup = parent,
                NatureId = row.NatureId,
                MainGroupId = row.MainGroupId,
                ImageId = row.ImageId,
                AccessFlag = row.AccessFlag,
                SortOrder = row.SortOrder
            };
        }

        private static List<int> ParseGroupIds(string groupIds)
        {
            List<int> ids = new List<int>();
            if (string.IsNullOrWhiteSpace(groupIds))
            {
                return ids;
            }

            string[] parts = groupIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                int id;
                if (int.TryParse(parts[i].Trim(), out id) && id > 0 && !ids.Contains(id))
                {
                    ids.Add(id);
                }
            }

            return ids;
        }

        private static async Task<int> ComputeSortOrder(
            MastersDbContext tenant,
            int parentGroupId,
            int natureId,
            CancellationToken cancellationToken)
        {
            int? maxUnderParent = await tenant.LedgerGroups
                .Where(row => row.ParentGroupId == parentGroupId)
                .MaxAsync(row => (int?)row.SortOrder, cancellationToken);

            if (parentGroupId == natureId)
            {
                if (maxUnderParent.HasValue && maxUnderParent.Value != 0)
                {
                    return maxUnderParent.Value + 100;
                }

                MasterLedgerGroup? parent = await tenant.LedgerGroups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(row => row.GroupId == parentGroupId, cancellationToken);
                int baseOrder = parent == null ? 0 : parent.SortOrder;
                return baseOrder + 100;
            }

            if (maxUnderParent.HasValue && maxUnderParent.Value != 0)
            {
                return maxUnderParent.Value + 1;
            }

            MasterLedgerGroup? main = await tenant.LedgerGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(row => row.GroupId == parentGroupId, cancellationToken);
            int mainOrder = main == null ? 0 : main.SortOrder;
            return mainOrder + 1;
        }
        #endregion

        #region Implementation of ILedgerGroupRepository
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

        public async Task<IReadOnlyList<LedgerGroupListItemDto>> GetLedgerGroupSource(
            HeadOfficeConnection office,
            CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterLedgerGroup> rows = await tenant.LedgerGroups
                    .AsNoTracking()
                    .OrderBy(row => row.LedgerGroup)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> names = rows.ToDictionary(
                    row => row.GroupId,
                    row => row.LedgerGroup ?? string.Empty);
                return rows.Select(row => ToItem(row, names)).ToList();
            }
        }

        public async Task<IReadOnlyList<LedgerGroupOptionDto>> LoadLedgerGroupSource(
            HeadOfficeConnection office,
            CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                // Old FetchforLookup excludes Bank(12), Cash(13), FD(14).
                return await tenant.LedgerGroups
                    .AsNoTracking()
                    .Where(row => row.GroupId != 12 && row.GroupId != 13 && row.GroupId != 14)
                    .OrderBy(row => row.LedgerGroup)
                    .Select(row => new LedgerGroupOptionDto
                    {
                        GroupId = row.GroupId,
                        LedgerGroup = row.LedgerGroup
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<MasterLedgerGroup?> FillLedgerGroupDetails(
            HeadOfficeConnection office,
            int groupId,
            CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.LedgerGroups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(row => row.GroupId == groupId, cancellationToken);
            }
        }

        public async Task<IReadOnlyList<LedgerUnderGroupDto>> GetLedgerList(
            HeadOfficeConnection office,
            string groupIds,
            CancellationToken cancellationToken)
        {
            List<int> selected = ParseGroupIds(groupIds);
            if (selected.Count == 0)
            {
                return Array.Empty<LedgerUnderGroupDto>();
            }

            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterLedgerGroup> all = await tenant.LedgerGroups
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                HashSet<int> scope = new HashSet<int>(selected);
                for (int pass = 0; pass < 3; pass++)
                {
                    for (int i = 0; i < all.Count; i++)
                    {
                        if (scope.Contains(all[i].ParentGroupId))
                        {
                            scope.Add(all[i].GroupId);
                        }
                    }
                }

                return await tenant.Ledgers
                    .AsNoTracking()
                    .Where(ledger => scope.Contains(ledger.GroupId))
                    .OrderBy(ledger => ledger.LedgerName)
                    .Select(ledger => new LedgerUnderGroupDto
                    {
                        LedgerCode = ledger.LedgerCode ?? string.Empty,
                        LedgerName = ledger.LedgerName
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<LedgerGroupActionResult> SaveLedgerGroupDetails(
            HeadOfficeConnection office,
            LedgerGroupSaveRequest request,
            CancellationToken cancellationToken)
        {
            string code = (request.Abbrevation ?? string.Empty).Trim().ToUpperInvariant();
            string name = (request.Group ?? string.Empty).Trim();

            if (request.ParentGroupId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_PARENT_EMPTY));
            }

            if (code.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_CODE_EMPTY));
            }

            if (code.Length > 10)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_CODE_MAX));
            }

            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_NAME_EMPTY));
            }

            if (name.Length > 100)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_NAME_MAX));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterLedgerGroup? parent = await tenant.LedgerGroups
                        .AsNoTracking()
                        .FirstOrDefaultAsync(row => row.GroupId == request.ParentGroupId, cancellationToken);
                    if (parent == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_PARENT_EMPTY));
                    }

                    // Only one level under a primary group (PARENT_GROUP_ID == NATURE_ID).
                    if (parent.ParentGroupId != parent.NatureId)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_LEVEL_CHECK));
                    }

                    string lower = name.ToLowerInvariant();
                    bool duplicate = await tenant.LedgerGroups.AnyAsync(
                        row => row.LedgerGroup.ToLower() == lower && row.GroupId != request.GroupId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_EXISTS));
                    }

                    if (request.GroupId <= 0)
                    {
                        int sortOrder = await ComputeSortOrder(tenant, request.ParentGroupId, parent.NatureId, cancellationToken);
                        MasterLedgerGroup row = new MasterLedgerGroup
                        {
                            GroupCode = code,
                            LedgerGroup = name,
                            ParentGroupId = request.ParentGroupId,
                            NatureId = parent.NatureId,
                            MainGroupId = request.ParentGroupId,
                            ImageId = 1,
                            AccessFlag = (int)AccessFlag.Accessable,
                            SortOrder = sortOrder
                        };
                        tenant.LedgerGroups.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_SAVE_SUCCESS), row.GroupId);
                    }

                    MasterLedgerGroup? existing = await tenant.LedgerGroups.FirstOrDefaultAsync(
                        row => row.GroupId == request.GroupId,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_NOT_FOUND));
                    }

                    existing.GroupCode = code;
                    existing.LedgerGroup = name;
                    existing.ParentGroupId = request.ParentGroupId;
                    existing.NatureId = parent.NatureId;
                    existing.MainGroupId = request.ParentGroupId;
                    existing.ImageId = 1;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_SAVE_SUCCESS), existing.GroupId);
                }
            }
            catch (DbUpdateException)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_EXISTS));
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<LedgerGroupActionResult> DeleteLedgerGroup(
            HeadOfficeConnection office,
            int groupId,
            CancellationToken cancellationToken)
        {
            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterLedgerGroup? existing = await tenant.LedgerGroups.FirstOrDefaultAsync(
                        row => row.GroupId == groupId,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_NOT_FOUND));
                    }

                    if (existing.AccessFlag == (int)AccessFlag.Readonly || existing.AccessFlag == (int)AccessFlag.Editable)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_NATURE_DELETE));
                    }

                    bool hasChildren = await tenant.LedgerGroups.AnyAsync(
                        row => row.ParentGroupId == groupId && row.GroupId != groupId,
                        cancellationToken);
                    if (hasChildren)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_CAN_DELETE));
                    }

                    bool hasLedgers = await tenant.Ledgers.AnyAsync(row => row.GroupId == groupId, cancellationToken);
                    if (hasLedgers)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_CAN_DELETE));
                    }

                    tenant.LedgerGroups.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_DELETE_SUCCESS), groupId);
                }
            }
            catch (DbUpdateException)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_CAN_DELETE));
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
        #endregion
    }
}
