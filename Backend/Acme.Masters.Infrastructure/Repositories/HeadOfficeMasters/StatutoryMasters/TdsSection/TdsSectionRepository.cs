/*  Class Name      : TdsSectionRepository.cs
 *  Purpose         : Head Office TDS Section master - list / fill / save / delete against the HO tenant
 *                    database (legacy TDSSQL.FetchTDSSection; rules: code + name required,
 *                    unique code / name, no deactivate or delete while a nature of payment uses it)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.TdsSection
{
    public class TdsSectionRepository : ITdsSectionRepository
    {
        private const int CodeMaxLength = 45;
        private const int NameMaxLength = 100;
        private const int ActiveStatus = 1;
        private const int InactiveStatus = 0;

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public TdsSectionRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static TdsSectionActionResult Ok(string message, int id)
        {
            return new TdsSectionActionResult { Success = true, Message = message, TdsSectionId = id };
        }

        private static TdsSectionActionResult Fail(string message)
        {
            return new TdsSectionActionResult { Success = false, Message = message };
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

        public async Task<IReadOnlyList<TdsSectionListItemDto>> FetchTdsSectionDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterTdsSection> rows = await tenant.Set<MasterTdsSection>().AsNoTracking()
                    .OrderBy(row => row.Code)
                    .ToListAsync(cancellationToken);
                return rows.Select(ToDto).ToList();
            }
        }

        public async Task<TdsSectionListItemDto?> FillTdsSectionDetails(HeadOfficeConnection office, int tdsSectionId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                MasterTdsSection? row = await tenant.Set<MasterTdsSection>().AsNoTracking()
                    .FirstOrDefaultAsync(item => item.TdsSectionId == tdsSectionId, cancellationToken);
                return row == null ? null : ToDto(row);
            }
        }

        public async Task<TdsSectionActionResult> SaveTdsSectionDetails(HeadOfficeConnection office, TdsSectionSaveRequest request, CancellationToken cancellationToken)
        {
            string code = (request.Code ?? string.Empty).Trim();
            string name = (request.SectionName ?? string.Empty).Trim();
            if (code.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSSection.TDS_CODE_EMPTY));
            }

            if (code.Length > CodeMaxLength)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_CODE_MAX));
            }

            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSSection.TDS_NAME_EMPTY));
            }

            if (name.Length > NameMaxLength)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_NAME_MAX));
            }

            if (request.Status != ActiveStatus && request.Status != InactiveStatus)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_STATUS_INVALID));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    string lowerCode = code.ToLower();
                    bool codeExists = await tenant.Set<MasterTdsSection>().AnyAsync(
                        row => row.Code.Trim().ToLower() == lowerCode && row.TdsSectionId != request.TdsSectionId,
                        cancellationToken);
                    if (codeExists)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_CODE_EXISTS));
                    }

                    string lowerName = name.ToLower();
                    bool nameExists = await tenant.Set<MasterTdsSection>().AnyAsync(
                        row => row.SectionName.Trim().ToLower() == lowerName && row.TdsSectionId != request.TdsSectionId,
                        cancellationToken);
                    if (nameExists)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_NAME_EXISTS));
                    }

                    if (request.TdsSectionId <= 0)
                    {
                        MasterTdsSection row = new MasterTdsSection
                        {
                            Code = code,
                            SectionName = name,
                            Status = request.Status
                        };
                        tenant.Set<MasterTdsSection>().Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.TDS.TDSSection.TDS_SAVE_SUCCESS), row.TdsSectionId);
                    }

                    MasterTdsSection? existing = await tenant.Set<MasterTdsSection>().FirstOrDefaultAsync(
                        row => row.TdsSectionId == request.TdsSectionId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_NOT_FOUND));
                    }

                    // A section still mapped to an active nature of payment cannot be made inactive.
                    if (existing.Status == ActiveStatus && request.Status == InactiveStatus)
                    {
                        bool usedByActiveNature = await tenant.Set<MasterTdsNaturePayment>().AnyAsync(
                            row => row.TdsSectionId == existing.TdsSectionId && row.Status == ActiveStatus,
                            cancellationToken);
                        if (usedByActiveNature)
                        {
                            return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSSection.TDS_CANNOT_SET_INACTIVE));
                        }
                    }

                    existing.Code = code;
                    existing.SectionName = name;
                    existing.Status = request.Status;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.TDS.TDSSection.TDS_SAVE_SUCCESS), existing.TdsSectionId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<TdsSectionActionResult> DeleteTdsSectionDetails(HeadOfficeConnection office, int tdsSectionId, CancellationToken cancellationToken)
        {
            if (tdsSectionId <= 0)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterTdsSection? existing = await tenant.Set<MasterTdsSection>().FirstOrDefaultAsync(
                        row => row.TdsSectionId == tdsSectionId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_NOT_FOUND));
                    }

                    bool inUse = await tenant.Set<MasterTdsNaturePayment>().AnyAsync(
                        row => row.TdsSectionId == tdsSectionId, cancellationToken);
                    if (inUse)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSSection.TDS_SECTION_DELETE_INFO));
                    }

                    tenant.Set<MasterTdsSection>().Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_DELETE_SUCCESS), tdsSectionId);
                }
            }
            catch (Exception)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_SECTION_DELETE_FAILURE));
            }
        }

        private static TdsSectionListItemDto ToDto(MasterTdsSection row)
        {
            return new TdsSectionListItemDto
            {
                TdsSectionId = row.TdsSectionId,
                Code = (row.Code ?? string.Empty).Trim(),
                SectionName = (row.SectionName ?? string.Empty).Trim(),
                Status = row.Status,
                StatusName = row.Status == ActiveStatus ? "Active" : "Inactive"
            };
        }
    }
}
