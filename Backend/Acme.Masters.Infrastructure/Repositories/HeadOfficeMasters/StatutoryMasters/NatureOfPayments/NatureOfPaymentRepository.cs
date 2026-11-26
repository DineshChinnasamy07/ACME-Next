/*  Class Name      : NatureOfPaymentRepository.cs
 *  Purpose         : Head Office TDS Nature of Payments master - list / section lookup / fill / save / delete
 *                    against the HO tenant database (legacy NatureofPaymentSQL / TDSSQL.FetchNatureofPaymentsSection;
 *                    rules: name, payment code and section required, unique name, active section only,
 *                    no deactivate or delete while TDS policy or a creditor profile uses it)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public class NatureOfPaymentRepository : INatureOfPaymentRepository
    {
        private const int NameMaxLength = 150;
        private const int PaymentCodeMaxLength = 7;
        private const int DescriptionMaxLength = 400;
        private const int ActiveStatus = 1;
        private const int InactiveStatus = 0;

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public NatureOfPaymentRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static NatureOfPaymentActionResult Ok(string message, int id)
        {
            return new NatureOfPaymentActionResult { Success = true, Message = message, NaturePayId = id };
        }

        private static NatureOfPaymentActionResult Fail(string message)
        {
            return new NatureOfPaymentActionResult { Success = false, Message = message };
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

        public async Task<IReadOnlyList<NatureOfPaymentListItemDto>> FetchNatureOfPaymentDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterTdsNaturePayment> rows = await tenant.Set<MasterTdsNaturePayment>().AsNoTracking()
                    .OrderBy(row => row.PaymentCode)
                    .ThenBy(row => row.Name)
                    .ToListAsync(cancellationToken);
                Dictionary<int, MasterTdsSection> sections = await tenant.Set<MasterTdsSection>().AsNoTracking()
                    .ToDictionaryAsync(row => row.TdsSectionId, cancellationToken);
                return rows.Select(row => ToDto(row, sections)).ToList();
            }
        }

        public async Task<IReadOnlyList<NatureOfPaymentSectionOptionDto>> FetchSectionCodes(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterTdsSection> rows = await tenant.Set<MasterTdsSection>().AsNoTracking()
                    .Where(row => row.Status == ActiveStatus)
                    .OrderBy(row => row.Code)
                    .ToListAsync(cancellationToken);
                return rows.Select(row => new NatureOfPaymentSectionOptionDto
                {
                    TdsSectionId = row.TdsSectionId,
                    Code = (row.Code ?? string.Empty).Trim(),
                    Name = (row.Code ?? string.Empty).Trim() + " - " + (row.SectionName ?? string.Empty).Trim()
                }).ToList();
            }
        }

        public async Task<NatureOfPaymentListItemDto?> FillNatureOfPaymentDetails(HeadOfficeConnection office, int naturePayId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                MasterTdsNaturePayment? row = await tenant.Set<MasterTdsNaturePayment>().AsNoTracking()
                    .FirstOrDefaultAsync(item => item.NaturePayId == naturePayId, cancellationToken);
                if (row == null)
                {
                    return null;
                }

                Dictionary<int, MasterTdsSection> sections = await tenant.Set<MasterTdsSection>().AsNoTracking()
                    .Where(item => item.TdsSectionId == row.TdsSectionId)
                    .ToDictionaryAsync(item => item.TdsSectionId, cancellationToken);
                return ToDto(row, sections);
            }
        }

        public async Task<NatureOfPaymentActionResult> SaveNatureOfPaymentDetails(HeadOfficeConnection office, NatureOfPaymentSaveRequest request, CancellationToken cancellationToken)
        {
            string name = (request.Name ?? string.Empty).Trim();
            string paymentCode = (request.PaymentCode ?? string.Empty).Trim();
            string description = (request.Description ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.NatureofPayments.TDS_NATUREPAYMENT_PAYMENT_NAME_EMPTY));
            }

            if (name.Length > NameMaxLength)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_NAME_MAX));
            }

            if (paymentCode.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.NatureofPayments.TDS_NATUREPAYMENT_PAYMENT_CODE_EMPTY));
            }

            if (paymentCode.Length > PaymentCodeMaxLength)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_CODE_MAX));
            }

            if (request.TdsSectionId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.NatureofPayments.TDS_NATUREOFPAYEMNTS_SECTION_EMPTY));
            }

            if (description.Length > DescriptionMaxLength)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_DESCRIPTION_MAX));
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
                    bool duplicate = await tenant.Set<MasterTdsNaturePayment>().AnyAsync(
                        row => row.Name.Trim().ToLower() == lowerName && row.NaturePayId != request.NaturePayId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_EXISTS));
                    }

                    MasterTdsSection? section = await tenant.Set<MasterTdsSection>().AsNoTracking()
                        .FirstOrDefaultAsync(row => row.TdsSectionId == request.TdsSectionId, cancellationToken);

                    if (request.NaturePayId <= 0)
                    {
                        if (section == null || section.Status != ActiveStatus)
                        {
                            return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_SECTION_INVALID));
                        }

                        MasterTdsNaturePayment row = new MasterTdsNaturePayment
                        {
                            Name = name,
                            PaymentCode = paymentCode,
                            TdsSectionId = request.TdsSectionId,
                            Description = description,
                            Status = request.Status
                        };
                        tenant.Set<MasterTdsNaturePayment>().Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_SAVE_SUCCESS), row.NaturePayId);
                    }

                    MasterTdsNaturePayment? existing = await tenant.Set<MasterTdsNaturePayment>().FirstOrDefaultAsync(
                        row => row.NaturePayId == request.NaturePayId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_NOT_FOUND));
                    }

                    // An inactive section may stay on a record that already had it, but cannot be newly chosen.
                    if (section == null || (section.Status != ActiveStatus && section.TdsSectionId != existing.TdsSectionId))
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_SECTION_INVALID));
                    }

                    if (existing.Status == ActiveStatus && request.Status == InactiveStatus
                        && await IsNatureOfPaymentInUse(tenant, existing.NaturePayId, cancellationToken))
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.TDS.NatureofPayments.TDS_CANNOT_SET_INACTIVE_INFO));
                    }

                    existing.Name = name;
                    existing.PaymentCode = paymentCode;
                    existing.TdsSectionId = request.TdsSectionId;
                    existing.Description = description;
                    existing.Status = request.Status;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_SAVE_SUCCESS), existing.NaturePayId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<NatureOfPaymentActionResult> DeleteNatureOfPaymentDetails(HeadOfficeConnection office, int naturePayId, CancellationToken cancellationToken)
        {
            if (naturePayId <= 0)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterTdsNaturePayment? existing = await tenant.Set<MasterTdsNaturePayment>().FirstOrDefaultAsync(
                        row => row.NaturePayId == naturePayId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_NOT_FOUND));
                    }

                    if (await IsNatureOfPaymentInUse(tenant, naturePayId, cancellationToken))
                    {
                        return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_IN_USE));
                    }

                    tenant.Set<MasterTdsNaturePayment>().Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_DELETE_SUCCESS), naturePayId);
                }
            }
            catch (Exception)
            {
                return Fail(MessageRender.GetMessage(TdsMasterMessages.TDS_NATURE_PAYMENT_DELETE_FAILURE));
            }
        }

        // Used by a TDS policy (tds_policy.TDS_NATURE_PAYMENT_ID) or a creditor's TDS profile
        // (tds_credtiors_profile.NATURE_OF_PAYMENT_ID) - tables owned by other masters, so plain existence queries.
        private static async Task<bool> IsNatureOfPaymentInUse(MastersDbContext tenant, int naturePayId, CancellationToken cancellationToken)
        {
            List<int> policyRows = await tenant.Database
                .SqlQuery<int>($"SELECT 1 AS \"Value\" FROM public.tds_policy WHERE \"TDS_NATURE_PAYMENT_ID\" = {naturePayId} LIMIT 1")
                .ToListAsync(cancellationToken);
            if (policyRows.Count > 0)
            {
                return true;
            }

            List<int> profileRows = await tenant.Database
                .SqlQuery<int>($"SELECT 1 AS \"Value\" FROM public.tds_credtiors_profile WHERE \"NATURE_OF_PAYMENT_ID\" = {naturePayId} LIMIT 1")
                .ToListAsync(cancellationToken);
            return profileRows.Count > 0;
        }

        private static NatureOfPaymentListItemDto ToDto(MasterTdsNaturePayment row, IReadOnlyDictionary<int, MasterTdsSection> sections)
        {
            MasterTdsSection? section;
            sections.TryGetValue(row.TdsSectionId, out section);
            return new NatureOfPaymentListItemDto
            {
                NaturePayId = row.NaturePayId,
                PaymentCode = (row.PaymentCode ?? string.Empty).Trim(),
                Name = (row.Name ?? string.Empty).Trim(),
                TdsSectionId = row.TdsSectionId,
                SectionCode = section == null ? string.Empty : (section.Code ?? string.Empty).Trim(),
                SectionName = section == null ? string.Empty : (section.SectionName ?? string.Empty).Trim(),
                Description = row.Description ?? string.Empty,
                Status = row.Status,
                StatusName = row.Status == ActiveStatus ? "Active" : "Inactive"
            };
        }
    }
}
