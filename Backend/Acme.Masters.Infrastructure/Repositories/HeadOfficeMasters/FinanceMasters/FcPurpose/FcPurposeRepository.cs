using Acme.Common;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.FcPurpose
{
    public class FcPurposeRepository : IFcPurposeRepository
    {
        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public FcPurposeRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static FcPurposeActionResult Ok(string message, int id)
        {
            return new FcPurposeActionResult { Success = true, Message = message, ContributionId = id };
        }

        private static FcPurposeActionResult Fail(string message)
        {
            return new FcPurposeActionResult { Success = false, Message = message };
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

        public async Task<IReadOnlyList<FcPurposeListItemDto>> FetchPurposeDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.ContributionHeads.AsNoTracking()
                    .OrderBy(row => row.FcPurpose)
                    .Select(row => new FcPurposeListItemDto
                    {
                        ContributionId = row.ContributionId,
                        Code = row.Code ?? string.Empty,
                        FcPurpose = row.FcPurpose ?? string.Empty
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<MasterContributionHead?> FillPurposeDetails(HeadOfficeConnection office, int contributionId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.ContributionHeads.AsNoTracking()
                    .FirstOrDefaultAsync(row => row.ContributionId == contributionId, cancellationToken);
            }
        }

        public async Task<FcPurposeActionResult> SavePurposeDetails(HeadOfficeConnection office, FcPurposeSaveRequest request, CancellationToken cancellationToken)
        {
            string code = (request.Code ?? string.Empty).Trim();
            string purpose = (request.FcPurpose ?? string.Empty).Trim();
            if (code.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_CODE_EMPTY));
            }

            if (code.Length > 10)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_CODE_MAX));
            }

            if (purpose.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_HEAD_EMPTY));
            }

            if (purpose.Length > 150)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_HEAD_MAX));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    string lower = purpose.ToLowerInvariant();
                    bool duplicate = await tenant.ContributionHeads.AnyAsync(
                        row => (row.FcPurpose ?? string.Empty).ToLower() == lower
                            && row.ContributionId != request.ContributionId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_EXISTS));
                    }

                    if (request.ContributionId <= 0)
                    {
                        MasterContributionHead row = new MasterContributionHead
                        {
                            Code = code,
                            FcPurpose = purpose
                        };
                        tenant.ContributionHeads.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_SAVE_SUCCESS), row.ContributionId);
                    }

                    MasterContributionHead? existing = await tenant.ContributionHeads.FirstOrDefaultAsync(
                        row => row.ContributionId == request.ContributionId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_NOT_FOUND));
                    }

                    existing.Code = code;
                    existing.FcPurpose = purpose;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_SAVE_SUCCESS), existing.ContributionId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<FcPurposeActionResult> DeletePurposeDetails(HeadOfficeConnection office, int contributionId, CancellationToken cancellationToken)
        {
            if (contributionId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterContributionHead? existing = await tenant.ContributionHeads.FirstOrDefaultAsync(
                        row => row.ContributionId == contributionId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_NOT_FOUND));
                    }

                    tenant.ContributionHeads.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_DELETE_SUCCESS), contributionId);
                }
            }
            catch (Exception)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_DELETE_FAILURE));
            }
        }
    }
}
