/*  Class Name      : DutyTaxRepository.cs
 *  Purpose         : Head Office TDS Duty / Tax type master - list / fill / save / delete against the HO tenant
 *                    database (legacy TDSSQL.FetchDutyTax; rules: name required and unique, built-in types 1-5
 *                    cannot be renamed or deleted, no delete while a TDS tax rate uses it)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.DutyTax
{
    public class DutyTaxRepository : IDutyTaxRepository
    {
        private const int NameMaxLength = 50;
        private const int ActiveStatus = 1;
        private const int InactiveStatus = 0;

        // TDS With PAN (1), TDS Without PAN (2), Surcharge (3), Ed Cess (4), Sec Ed Cess (5): the legacy voucher
        // TDS SQL reads TDS_TAX_TYPE_ID 1..5 by number, so these rows must keep their meaning.
        private const int LastSystemTaxTypeId = 5;

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public DutyTaxRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static DutyTaxActionResult Ok(string message, int id)
        {
            return new DutyTaxActionResult { Success = true, Message = message, TdsDutyTaxTypeId = id };
        }

        private static DutyTaxActionResult Fail(string message)
        {
            return new DutyTaxActionResult { Success = false, Message = message };
        }

        private static bool IsSystemTaxType(int tdsDutyTaxTypeId)
        {
            return tdsDutyTaxTypeId > 0 && tdsDutyTaxTypeId <= LastSystemTaxTypeId;
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

        public async Task<IReadOnlyList<DutyTaxListItemDto>> FetchDutyTaxDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterTdsDutyTaxType> rows = await tenant.Set<MasterTdsDutyTaxType>().AsNoTracking()
                    .OrderBy(row => row.TdsDutyTaxTypeId)
                    .ToListAsync(cancellationToken);
                return rows.Select(ToDto).ToList();
            }
        }

        public async Task<DutyTaxListItemDto?> FillDutyTaxDetails(HeadOfficeConnection office, int tdsDutyTaxTypeId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                MasterTdsDutyTaxType? row = await tenant.Set<MasterTdsDutyTaxType>().AsNoTracking()
                    .FirstOrDefaultAsync(item => item.TdsDutyTaxTypeId == tdsDutyTaxTypeId, cancellationToken);
                return row == null ? null : ToDto(row);
            }
        }

        public async Task<DutyTaxActionResult> SaveDutyTaxDetails(HeadOfficeConnection office, DutyTaxSaveRequest request, CancellationToken cancellationToken)
        {
            string name = (request.TaxTypeName ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.DutyTax.TDS_DUTY_TAX_NAME_EMPTY));
            }

            if (name.Length > NameMaxLength)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_NAME_MAX));
            }

            if (request.Status != ActiveStatus && request.Status != InactiveStatus)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_STATUS_INVALID));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    string lowerName = name.ToLower();
                    bool duplicate = await tenant.Set<MasterTdsDutyTaxType>().AnyAsync(
                        row => row.TaxTypeName.Trim().ToLower() == lowerName && row.TdsDutyTaxTypeId != request.TdsDutyTaxTypeId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_EXISTS));
                    }

                    if (request.TdsDutyTaxTypeId <= 0)
                    {
                        MasterTdsDutyTaxType row = new MasterTdsDutyTaxType
                        {
                            TaxTypeName = name,
                            Status = request.Status
                        };
                        tenant.Set<MasterTdsDutyTaxType>().Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_SAVE_SUCCESS), row.TdsDutyTaxTypeId);
                    }

                    MasterTdsDutyTaxType? existing = await tenant.Set<MasterTdsDutyTaxType>().FirstOrDefaultAsync(
                        row => row.TdsDutyTaxTypeId == request.TdsDutyTaxTypeId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_NOT_FOUND));
                    }

                    // Built-in tax types keep their name; only their status may change.
                    if (IsSystemTaxType(existing.TdsDutyTaxTypeId)
                        && !string.Equals((existing.TaxTypeName ?? string.Empty).Trim(), name, StringComparison.Ordinal))
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.TDS.DutyTax.TDS_DUTY_TAX_TYPE_CANNOT_EDIT_INFO));
                    }

                    existing.TaxTypeName = name;
                    existing.Status = request.Status;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_SAVE_SUCCESS), existing.TdsDutyTaxTypeId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<DutyTaxActionResult> DeleteDutyTaxDetails(HeadOfficeConnection office, int tdsDutyTaxTypeId, CancellationToken cancellationToken)
        {
            if (tdsDutyTaxTypeId <= 0)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_NOT_FOUND));
            }

            if (IsSystemTaxType(tdsDutyTaxTypeId))
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_SYSTEM_DELETE));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterTdsDutyTaxType? existing = await tenant.Set<MasterTdsDutyTaxType>().FirstOrDefaultAsync(
                        row => row.TdsDutyTaxTypeId == tdsDutyTaxTypeId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_NOT_FOUND));
                    }

                    // tds_tax_rate belongs to TDS Policy: plain existence query, no entity.
                    List<int> rateRows = await tenant.Database
                        .SqlQuery<int>($"SELECT 1 AS \"Value\" FROM public.tds_tax_rate WHERE \"TDS_TAX_TYPE_ID\" = {tdsDutyTaxTypeId} LIMIT 1")
                        .ToListAsync(cancellationToken);
                    if (rateRows.Count > 0)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_IN_USE));
                    }

                    tenant.Set<MasterTdsDutyTaxType>().Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_DELETE_SUCCESS), tdsDutyTaxTypeId);
                }
            }
            catch (Exception)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DUTY_TAX_DELETE_FAILURE));
            }
        }

        private static DutyTaxListItemDto ToDto(MasterTdsDutyTaxType row)
        {
            return new DutyTaxListItemDto
            {
                TdsDutyTaxTypeId = row.TdsDutyTaxTypeId,
                TaxTypeName = (row.TaxTypeName ?? string.Empty).Trim(),
                Status = row.Status,
                StatusName = row.Status == ActiveStatus ? "Active" : "Inactive",
                IsSystem = IsSystemTaxType(row.TdsDutyTaxTypeId)
            };
        }
    }
}
