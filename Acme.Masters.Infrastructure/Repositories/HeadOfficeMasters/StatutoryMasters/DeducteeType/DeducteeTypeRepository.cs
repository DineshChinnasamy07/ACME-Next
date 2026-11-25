/*  Class Name      : DeducteeTypeRepository.cs
 *  Purpose         : Head Office TDS Deductee Type master - list / fill / save / delete against the HO tenant
 *                    database (legacy DeducteeTypeSQL / TDSSQL.FetchDeducteeTypes; rules: name required and
 *                    unique, residential status 0/1, deductee status 0/1, no deactivate or delete while a
 *                    TDS policy or a creditor's TDS profile uses it)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.DeducteeType
{
    public class DeducteeTypeRepository : IDeducteeTypeRepository
    {
        private const int NameMaxLength = 100;
        private const int ActiveStatus = 1;
        private const int InactiveStatus = 0;

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public DeducteeTypeRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static DeducteeTypeActionResult Ok(string message, int id)
        {
            return new DeducteeTypeActionResult { Success = true, Message = message, DeducteeTypeId = id };
        }

        private static DeducteeTypeActionResult Fail(string message)
        {
            return new DeducteeTypeActionResult { Success = false, Message = message };
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

        public async Task<IReadOnlyList<DeducteeTypeListItemDto>> FetchDeducteeTypeDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterTdsDeducteeType> rows = await tenant.Set<MasterTdsDeducteeType>().AsNoTracking()
                    .OrderBy(row => row.Name)
                    .ToListAsync(cancellationToken);
                return rows.Select(ToDto).ToList();
            }
        }

        public async Task<DeducteeTypeListItemDto?> FillDeducteeTypeDetails(HeadOfficeConnection office, int deducteeTypeId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                MasterTdsDeducteeType? row = await tenant.Set<MasterTdsDeducteeType>().AsNoTracking()
                    .FirstOrDefaultAsync(item => item.DeducteeTypeId == deducteeTypeId, cancellationToken);
                return row == null ? null : ToDto(row);
            }
        }

        public async Task<DeducteeTypeActionResult> SaveDeducteeTypeDetails(HeadOfficeConnection office, DeducteeTypeSaveRequest request, CancellationToken cancellationToken)
        {
            string name = (request.Name ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.DeducteeTypes.TDS_DEDUCTEE_NAME_EMPTY));
            }

            if (name.Length > NameMaxLength)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_NAME_MAX));
            }

            if (request.ResidentialStatus != (int)TDSResidentialStatus.Residential
                && request.ResidentialStatus != (int)TDSResidentialStatus.Non_Residential)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_RESIDENTIAL_STATUS_INVALID));
            }

            if (request.DeducteeType != (int)TDSDeducteeStatus.Company
                && request.DeducteeType != (int)TDSDeducteeStatus.Non_Company)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.DeducteeTypes.TDS_DEDUCTEE_TYPE_EMPTY));
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
                    bool duplicate = await tenant.Set<MasterTdsDeducteeType>().AnyAsync(
                        row => row.Name.Trim().ToLower() == lowerName && row.DeducteeTypeId != request.DeducteeTypeId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_TYPE_EXISTS));
                    }

                    if (request.DeducteeTypeId <= 0)
                    {
                        MasterTdsDeducteeType row = new MasterTdsDeducteeType
                        {
                            Name = name,
                            ResidentialStatus = request.ResidentialStatus,
                            DeducteeType = request.DeducteeType,
                            Status = request.Status
                        };
                        tenant.Set<MasterTdsDeducteeType>().Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.TDS.NatureofPayments.TDS_DEDUCTEE_TYPE_SUCCESS), row.DeducteeTypeId);
                    }

                    MasterTdsDeducteeType? existing = await tenant.Set<MasterTdsDeducteeType>().FirstOrDefaultAsync(
                        row => row.DeducteeTypeId == request.DeducteeTypeId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_TYPE_NOT_FOUND));
                    }

                    if (existing.Status == ActiveStatus && request.Status == InactiveStatus
                        && await IsDeducteeTypeInUse(tenant, existing.DeducteeTypeId, cancellationToken))
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.TDS.DeducteeTypes.TDS_DEDUCTEE_TYPES_INACTIVE_INFO));
                    }

                    existing.Name = name;
                    existing.ResidentialStatus = request.ResidentialStatus;
                    existing.DeducteeType = request.DeducteeType;
                    existing.Status = request.Status;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.TDS.NatureofPayments.TDS_DEDUCTEE_TYPE_SUCCESS), existing.DeducteeTypeId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<DeducteeTypeActionResult> DeleteDeducteeTypeDetails(HeadOfficeConnection office, int deducteeTypeId, CancellationToken cancellationToken)
        {
            if (deducteeTypeId <= 0)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_TYPE_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterTdsDeducteeType? existing = await tenant.Set<MasterTdsDeducteeType>().FirstOrDefaultAsync(
                        row => row.DeducteeTypeId == deducteeTypeId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_TYPE_NOT_FOUND));
                    }

                    if (await IsDeducteeTypeInUse(tenant, deducteeTypeId, cancellationToken))
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_TYPE_IN_USE));
                    }

                    tenant.Set<MasterTdsDeducteeType>().Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_TYPE_DELETE_SUCCESS), deducteeTypeId);
                }
            }
            catch (Exception)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_DEDUCTEE_TYPE_DELETE_FAILURE));
            }
        }

        // Used by a TDS policy (tds_policy.TDS_DEDUCTEE_TYPE_ID) or a creditor's TDS profile
        // (tds_credtiors_profile.DEDUTEE_TYPE_ID, legacy spelling) - tables owned by other masters.
        private static async Task<bool> IsDeducteeTypeInUse(MastersDbContext tenant, int deducteeTypeId, CancellationToken cancellationToken)
        {
            List<int> policyRows = await tenant.Database
                .SqlQuery<int>($"SELECT 1 AS \"Value\" FROM public.tds_policy WHERE \"TDS_DEDUCTEE_TYPE_ID\" = {deducteeTypeId} LIMIT 1")
                .ToListAsync(cancellationToken);
            if (policyRows.Count > 0)
            {
                return true;
            }

            List<int> profileRows = await tenant.Database
                .SqlQuery<int>($"SELECT 1 AS \"Value\" FROM public.tds_credtiors_profile WHERE \"DEDUTEE_TYPE_ID\" = {deducteeTypeId} LIMIT 1")
                .ToListAsync(cancellationToken);
            return profileRows.Count > 0;
        }

        private static DeducteeTypeListItemDto ToDto(MasterTdsDeducteeType row)
        {
            return new DeducteeTypeListItemDto
            {
                DeducteeTypeId = row.DeducteeTypeId,
                Name = (row.Name ?? string.Empty).Trim(),
                ResidentialStatus = row.ResidentialStatus,
                ResidentialStatusName = row.ResidentialStatus == (int)TDSResidentialStatus.Residential ? "Resident" : "Non Resident",
                DeducteeType = row.DeducteeType,
                DeducteeTypeName = row.DeducteeType == (int)TDSDeducteeStatus.Company ? "Company" : "Non Company",
                Status = row.Status,
                StatusName = row.Status == ActiveStatus ? "Active" : "Inactive"
            };
        }
    }
}
