using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Infrastructure.Persistence;
using Bosco.Masters.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class LockVoucherRepository : ILockVoucherRepository
    {
        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public LockVoucherRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static LockVoucherActionResult Ok(string message, int id)
        {
            return new LockVoucherActionResult { Success = true, Message = message, LockTransId = id };
        }

        private static LockVoucherActionResult Fail(string message)
        {
            return new LockVoucherActionResult { Success = false, Message = message };
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

        private static DateTime? NormalizeDate(DateTime? value)
        {
            if (!value.HasValue || value.Value == DateTime.MinValue)
            {
                return null;
            }

            return value.Value.Date;
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

        public async Task<IReadOnlyList<LockVoucherListItemDto>> FetchLockedProjects(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterLockTrans> rows = await tenant.LockTrans.AsNoTracking()
                    .OrderByDescending(row => row.DateFrom)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> branches = await tenant.BranchOffices.AsNoTracking()
                    .ToDictionaryAsync(row => row.BranchOfficeId, row => row.BranchOfficeName, cancellationToken);
                Dictionary<int, string> projects = await tenant.Projects.AsNoTracking()
                    .ToDictionaryAsync(row => row.ProjectId, row => row.Project, cancellationToken);

                List<LockVoucherListItemDto> items = new List<LockVoucherListItemDto>(rows.Count);
                for (int i = 0; i < rows.Count; i++)
                {
                    MasterLockTrans row = rows[i];
                    string branchName;
                    string projectName;
                    branches.TryGetValue(row.BranchId, out branchName!);
                    projects.TryGetValue(row.ProjectId, out projectName!);
                    items.Add(new LockVoucherListItemDto
                    {
                        LockTransId = row.LockTransId,
                        BranchId = row.BranchId,
                        BranchOffice = branchName ?? string.Empty,
                        ProjectId = row.ProjectId,
                        Project = projectName ?? string.Empty,
                        DateFrom = row.DateFrom,
                        DateTo = row.DateTo,
                        Reason = row.Reason ?? string.Empty,
                        Locked = row.LockByPortal == 1
                    });
                }

                return items;
            }
        }

        public async Task<MasterLockTrans?> FillLockVoucherDetails(HeadOfficeConnection office, int lockTransId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.LockTrans.AsNoTracking()
                    .FirstOrDefaultAsync(row => row.LockTransId == lockTransId, cancellationToken);
            }
        }

        public async Task<IReadOnlyList<LockVoucherOptionDto>> FetchBranches(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.BranchOffices.AsNoTracking()
                    .OrderBy(row => row.BranchOfficeName)
                    .Select(row => new LockVoucherOptionDto
                    {
                        Id = row.BranchOfficeId,
                        Name = row.BranchOfficeName
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<LockVoucherOptionDto>> FetchProjectsForVoucherLock(HeadOfficeConnection office, int branchId, int lockTransId, CancellationToken cancellationToken)
        {
            if (branchId <= 0)
            {
                return Array.Empty<LockVoucherOptionDto>();
            }

            await using (MastersDbContext tenant = OpenTenant(office))
            {
                HashSet<int> mappedProjectIds = new HashSet<int>(
                    await tenant.ProjectBranches.AsNoTracking()
                        .Where(row => row.BranchId == branchId)
                        .Select(row => row.ProjectId)
                        .Distinct()
                        .ToListAsync(cancellationToken));

                HashSet<int> lockedProjectIds = new HashSet<int>(
                    await tenant.LockTrans.AsNoTracking()
                        .Where(row => row.LockTransId != lockTransId)
                        .Select(row => row.ProjectId)
                        .Distinct()
                        .ToListAsync(cancellationToken));

                return await tenant.Projects.AsNoTracking()
                    .Where(row => row.DeleteFlag != 1
                        && mappedProjectIds.Contains(row.ProjectId)
                        && (lockTransId > 0 || !lockedProjectIds.Contains(row.ProjectId)))
                    .OrderBy(row => row.Project)
                    .Select(row => new LockVoucherOptionDto
                    {
                        Id = row.ProjectId,
                        Name = row.Project
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<LockVoucherActionResult> SaveLockVoucher(HeadOfficeConnection office, LockVoucherSaveRequest request, CancellationToken cancellationToken)
        {
            if (request.BranchId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_BRANCH_EMPTY));
            }

            if (request.ProjectId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_PROJECT_EMPTY));
            }

            DateTime? dateFrom = NormalizeDate(request.DateFrom);
            DateTime? dateTo = NormalizeDate(request.DateTo);
            if (!dateFrom.HasValue)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.DATE_FROM_EMPTY));
            }

            if (!dateTo.HasValue)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.DATETOEMPTY));
            }

            if (dateFrom.Value.Date > DateTime.Today)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.DATE_FROM_FUTURE));
            }

            if (dateTo.Value.Date < dateFrom.Value.Date)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_CLOSE_DATE));
            }

            string password = (request.Password ?? string.Empty).Trim();
            string hint = (request.PasswordHint ?? string.Empty).Trim();
            bool isNew = request.LockTransId <= 0;
            if (isNew && password.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.PASSWORDEMPTY));
            }

            if (isNew && hint.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.PASSWORD_HINT_EMPTY));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    bool branchOk = await tenant.BranchOffices.AnyAsync(row => row.BranchOfficeId == request.BranchId, cancellationToken);
                    if (!branchOk)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_BRANCH_EMPTY));
                    }

                    bool projectOk = await tenant.Projects.AnyAsync(
                        row => row.ProjectId == request.ProjectId && row.DeleteFlag != 1, cancellationToken);
                    if (!projectOk)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_PROJECT_EMPTY));
                    }

                    if (isNew)
                    {
                        MasterLockTrans row = new MasterLockTrans
                        {
                            BranchId = request.BranchId,
                            ProjectId = request.ProjectId,
                            DateFrom = dateFrom.Value,
                            DateTo = dateTo.Value,
                            Password = LegacyPasswordCipher.EncryptValue(password),
                            PasswordHint = LegacyPasswordCipher.EncryptValue(hint),
                            Reason = (request.Reason ?? string.Empty).Trim(),
                            LockType = "Pre Audit",
                            LockByPortal = 1
                        };
                        tenant.LockTrans.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_SAVE_SUCCESS), row.LockTransId);
                    }

                    MasterLockTrans? existing = await tenant.LockTrans.FirstOrDefaultAsync(
                        row => row.LockTransId == request.LockTransId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_NOT_FOUND));
                    }

                    existing.BranchId = request.BranchId;
                    existing.ProjectId = request.ProjectId;
                    existing.DateFrom = dateFrom.Value;
                    existing.DateTo = dateTo.Value;
                    existing.Reason = (request.Reason ?? string.Empty).Trim();
                    existing.LockByPortal = 1;
                    if (password.Length > 0)
                    {
                        existing.Password = LegacyPasswordCipher.EncryptValue(password);
                    }

                    if (hint.Length > 0)
                    {
                        existing.PasswordHint = LegacyPasswordCipher.EncryptValue(hint);
                    }

                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_SAVE_SUCCESS), existing.LockTransId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<LockVoucherActionResult> DeleteLockDetails(HeadOfficeConnection office, int lockTransId, CancellationToken cancellationToken)
        {
            if (lockTransId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterLockTrans? existing = await tenant.LockTrans.FirstOrDefaultAsync(
                        row => row.LockTransId == lockTransId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_NOT_FOUND));
                    }

                    tenant.LockTrans.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.AuditLockType.LOCK_VOUCHER_DELETE_SUCCESS), lockTransId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
    }
}
