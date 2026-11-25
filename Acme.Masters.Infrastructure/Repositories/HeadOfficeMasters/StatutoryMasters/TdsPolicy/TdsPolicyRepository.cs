/*  Class Name      : TdsPolicyRepository.cs
 *  Purpose         : Head Office Statutory Masters - TDS Policy (TDS_POLICY + TDS_TAX_RATE) data access
 *                    on the head office tenant database
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsPolicyRepository : ITdsPolicyRepository
    {
        // numeric(10,2) columns: TDS_RATE is a percentage, TDS_EXEMPTION_LIMIT an amount.
        private const decimal MaxRate = 100m;
        private const decimal MaxExemptionLimit = 99999999.99m;

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public TdsPolicyRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static TdsPolicyActionResult Ok(string message, int id)
        {
            return new TdsPolicyActionResult { Success = true, Message = message, TdsPolicyId = id };
        }

        private static TdsPolicyActionResult Fail(string message)
        {
            return new TdsPolicyActionResult { Success = false, Message = message };
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

        private static string ResidentialStatusText(int value)
        {
            // Legacy FetchDeducteeTypes: 0 = Resident, otherwise Non Resident.
            return value == 0 ? "Resident" : "Non Resident";
        }

        private static string DeducteeKindText(int value)
        {
            // Legacy FetchDeducteeTypes: 0 = Company, otherwise Non Company.
            return value == 0 ? "Company" : "Non Company";
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

        public async Task<IReadOnlyList<TdsPolicyListItemDto>> FetchTdsPolicyDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterTdsPolicy> policies = await tenant.Set<MasterTdsPolicy>().AsNoTracking()
                    .ToListAsync(cancellationToken);
                List<MasterTdsTaxRate> rates = await tenant.Set<MasterTdsTaxRate>().AsNoTracking()
                    .OrderBy(row => row.TdsTaxTypeId)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> deductees = await tenant.Set<MasterTdsDeducteeType>().AsNoTracking()
                    .ToDictionaryAsync(row => row.DeducteeTypeId, row => row.Name, cancellationToken);
                Dictionary<int, MasterTdsNaturePayment> natures = await tenant.Set<MasterTdsNaturePayment>().AsNoTracking()
                    .ToDictionaryAsync(row => row.NaturePayId, cancellationToken);
                Dictionary<int, MasterTdsSection> sections = await tenant.Set<MasterTdsSection>().AsNoTracking()
                    .ToDictionaryAsync(row => row.TdsSectionId, cancellationToken);
                Dictionary<int, string> taxTypes = await tenant.Set<MasterTdsDutyTaxType>().AsNoTracking()
                    .ToDictionaryAsync(row => row.TdsDutyTaxTypeId, row => row.TaxTypeName, cancellationToken);

                ILookup<int, MasterTdsTaxRate> ratesByPolicy = rates.ToLookup(row => row.TdsPolicyId);
                List<TdsPolicyListItemDto> items = new List<TdsPolicyListItemDto>(policies.Count);
                for (int i = 0; i < policies.Count; i++)
                {
                    MasterTdsPolicy policy = policies[i];
                    string deducteeName;
                    MasterTdsNaturePayment? nature;
                    MasterTdsSection? section = null;
                    deductees.TryGetValue(policy.TdsDeducteeTypeId, out deducteeName!);
                    natures.TryGetValue(policy.TdsNaturePaymentId, out nature);
                    if (nature != null)
                    {
                        sections.TryGetValue(nature.TdsSectionId, out section);
                    }

                    items.Add(new TdsPolicyListItemDto
                    {
                        TdsPolicyId = policy.TdsPolicyId,
                        DeducteeTypeId = policy.TdsDeducteeTypeId,
                        DeducteeType = deducteeName ?? string.Empty,
                        NaturePayId = policy.TdsNaturePaymentId,
                        NatureOfPayment = nature != null ? nature.Name : string.Empty,
                        PaymentCode = nature != null ? (nature.PaymentCode ?? string.Empty) : string.Empty,
                        TdsSectionId = nature != null ? nature.TdsSectionId : 0,
                        SectionCode = section != null ? section.Code : (nature != null ? (nature.Section ?? string.Empty) : string.Empty),
                        SectionName = section != null ? section.SectionName : (nature != null ? (nature.SectionName ?? string.Empty) : string.Empty),
                        ApplicableFrom = policy.ApplicableFrom.Date,
                        TaxRates = ToRateDtos(ratesByPolicy[policy.TdsPolicyId], taxTypes)
                    });
                }

                return items
                    .OrderBy(row => row.DeducteeType, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(row => row.NatureOfPayment, StringComparer.OrdinalIgnoreCase)
                    .ThenByDescending(row => row.ApplicableFrom)
                    .ToList();
            }
        }

        private static List<TdsTaxRateDto> ToRateDtos(IEnumerable<MasterTdsTaxRate> rates, Dictionary<int, string> taxTypes)
        {
            List<TdsTaxRateDto> list = new List<TdsTaxRateDto>();
            foreach (MasterTdsTaxRate rate in rates)
            {
                string taxTypeName;
                taxTypes.TryGetValue(rate.TdsTaxTypeId, out taxTypeName!);
                list.Add(new TdsTaxRateDto
                {
                    TaxRateId = rate.TaxRateId,
                    TdsTaxTypeId = rate.TdsTaxTypeId,
                    TaxTypeName = taxTypeName ?? string.Empty,
                    TdsRate = rate.TdsRate,
                    TdsExemptionLimit = rate.TdsExemptionLimit
                });
            }

            return list;
        }

        public async Task<TdsPolicyLookupsDto> FetchTdsPolicyLookups(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterTdsDeducteeType> deductees = await tenant.Set<MasterTdsDeducteeType>().AsNoTracking()
                    .Where(row => row.Status == 1)
                    .OrderBy(row => row.Name)
                    .ToListAsync(cancellationToken);
                List<MasterTdsNaturePayment> natures = await tenant.Set<MasterTdsNaturePayment>().AsNoTracking()
                    .Where(row => row.Status == 1)
                    .OrderBy(row => row.Name)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> sectionCodes = await tenant.Set<MasterTdsSection>().AsNoTracking()
                    .ToDictionaryAsync(row => row.TdsSectionId, row => row.Code, cancellationToken);
                List<TdsPolicyOptionDto> taxTypes = await tenant.Set<MasterTdsDutyTaxType>().AsNoTracking()
                    .Where(row => row.Status == 1)
                    .OrderBy(row => row.TdsDutyTaxTypeId)
                    .Select(row => new TdsPolicyOptionDto
                    {
                        Id = row.TdsDutyTaxTypeId,
                        Name = row.TaxTypeName
                    })
                    .ToListAsync(cancellationToken);

                TdsPolicyLookupsDto lookups = new TdsPolicyLookupsDto { TaxTypes = taxTypes };
                for (int i = 0; i < deductees.Count; i++)
                {
                    MasterTdsDeducteeType row = deductees[i];
                    lookups.DeducteeTypes.Add(new TdsPolicyOptionDto
                    {
                        Id = row.DeducteeTypeId,
                        Name = row.Name,
                        Detail = ResidentialStatusText(row.ResidentialStatus) + " / " + DeducteeKindText(row.DeducteeType)
                    });
                }

                for (int i = 0; i < natures.Count; i++)
                {
                    MasterTdsNaturePayment row = natures[i];
                    string sectionCode;
                    if (!sectionCodes.TryGetValue(row.TdsSectionId, out sectionCode!) || string.IsNullOrWhiteSpace(sectionCode))
                    {
                        sectionCode = row.Section ?? string.Empty;
                    }

                    lookups.NatureOfPayments.Add(new TdsPolicyOptionDto
                    {
                        Id = row.NaturePayId,
                        Name = row.Name,
                        Detail = sectionCode
                    });
                }

                return lookups;
            }
        }

        public async Task<TdsPolicyDetailDto?> FillTdsPolicyDetails(HeadOfficeConnection office, int tdsPolicyId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                MasterTdsPolicy? policy = await tenant.Set<MasterTdsPolicy>().AsNoTracking()
                    .FirstOrDefaultAsync(row => row.TdsPolicyId == tdsPolicyId, cancellationToken);
                if (policy == null)
                {
                    return null;
                }

                List<MasterTdsTaxRate> rates = await tenant.Set<MasterTdsTaxRate>().AsNoTracking()
                    .Where(row => row.TdsPolicyId == tdsPolicyId)
                    .OrderBy(row => row.TdsTaxTypeId)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> taxTypes = await tenant.Set<MasterTdsDutyTaxType>().AsNoTracking()
                    .ToDictionaryAsync(row => row.TdsDutyTaxTypeId, row => row.TaxTypeName, cancellationToken);

                return new TdsPolicyDetailDto
                {
                    TdsPolicyId = policy.TdsPolicyId,
                    DeducteeTypeId = policy.TdsDeducteeTypeId,
                    NaturePayId = policy.TdsNaturePaymentId,
                    ApplicableFrom = policy.ApplicableFrom.Date,
                    TaxRates = ToRateDtos(rates, taxTypes)
                };
            }
        }

        public async Task<TdsPolicyActionResult> SaveTdsPolicyDetails(HeadOfficeConnection office, TdsPolicySaveRequest request, CancellationToken cancellationToken)
        {
            if (request.DeducteeTypeId <= 0)
            {
                return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_DEDUCTEE_TYPE_EMPTY));
            }

            if (request.NaturePayId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSPolicy.TDS_POLICY_NATURE_OF_PAYMENT_EMPTY));
            }

            if (!request.ApplicableFrom.HasValue || request.ApplicableFrom.Value == DateTime.MinValue)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSPolicy.TDS_APPLICABLE_FROM_EMPTY));
            }

            // Tax rate rows: one per duty / tax type, rate 0-100 %, exemption limit within numeric(10,2).
            // Rows with both rate and exemption limit 0 are not stored (legacy reports read a missing row as 0).
            List<TdsTaxRateSaveRow> rows = new List<TdsTaxRateSaveRow>();
            HashSet<int> seenTaxTypes = new HashSet<int>();
            List<TdsTaxRateSaveRow> requested = request.TaxRates ?? new List<TdsTaxRateSaveRow>();
            for (int i = 0; i < requested.Count; i++)
            {
                TdsTaxRateSaveRow row = requested[i];
                if (row == null)
                {
                    continue;
                }

                if (row.TdsTaxTypeId <= 0)
                {
                    return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSPolicy.TDS_TAX_TYPE_NOT_EXISTS));
                }

                if (!seenTaxTypes.Add(row.TdsTaxTypeId))
                {
                    return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_TAX_TYPE_DUPLICATE));
                }

                if (row.TdsRate < 0 || row.TdsRate > MaxRate || decimal.Round(row.TdsRate, 2) != row.TdsRate)
                {
                    return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_RATE_INVALID));
                }

                if (row.TdsExemptionLimit < 0 || row.TdsExemptionLimit > MaxExemptionLimit || decimal.Round(row.TdsExemptionLimit, 2) != row.TdsExemptionLimit)
                {
                    return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_EXEMPTION_INVALID));
                }

                if (row.TdsRate == 0 && row.TdsExemptionLimit == 0)
                {
                    continue;
                }

                rows.Add(row);
            }

            if (!rows.Any(row => row.TdsRate > 0))
            {
                return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_RATE_EMPTY));
            }

            // Tenant column is timestamp without time zone; EF maps DateTime as timestamptz, so write UTC-kind midnight.
            DateTime applicableFrom = DateTime.SpecifyKind(request.ApplicableFrom.Value.Date, DateTimeKind.Utc);
            DateTime applicableTo = applicableFrom.AddDays(1);
            bool isNew = request.TdsPolicyId <= 0;

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterTdsPolicy? existing = null;
                    HashSet<int> currentTaxTypes = new HashSet<int>();
                    if (!isNew)
                    {
                        existing = await tenant.Set<MasterTdsPolicy>().FirstOrDefaultAsync(
                            row => row.TdsPolicyId == request.TdsPolicyId, cancellationToken);
                        if (existing == null)
                        {
                            return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_NOT_FOUND));
                        }

                        currentTaxTypes = new HashSet<int>(await tenant.Set<MasterTdsTaxRate>().AsNoTracking()
                            .Where(row => row.TdsPolicyId == request.TdsPolicyId)
                            .Select(row => row.TdsTaxTypeId)
                            .ToListAsync(cancellationToken));
                    }

                    // A new policy needs an active deductee type / nature of payment; an edit may keep the stored one.
                    int deducteeTypeId = request.DeducteeTypeId;
                    int naturePayId = request.NaturePayId;
                    bool keepsDeductee = existing != null && existing.TdsDeducteeTypeId == deducteeTypeId;
                    bool keepsNature = existing != null && existing.TdsNaturePaymentId == naturePayId;
                    bool deducteeOk = await tenant.Set<MasterTdsDeducteeType>().AnyAsync(
                        row => row.DeducteeTypeId == deducteeTypeId && (keepsDeductee || row.Status == 1),
                        cancellationToken);
                    if (!deducteeOk)
                    {
                        return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_DEDUCTEE_TYPE_EMPTY));
                    }

                    bool natureOk = await tenant.Set<MasterTdsNaturePayment>().AnyAsync(
                        row => row.NaturePayId == naturePayId && (keepsNature || row.Status == 1),
                        cancellationToken);
                    if (!natureOk)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSPolicy.TDS_POLICY_NATURE_OF_PAYMENT_EMPTY));
                    }

                    // Tax types must be active duty / tax types (or already on this policy).
                    List<int> requestedTaxTypes = rows.Select(row => row.TdsTaxTypeId).ToList();
                    HashSet<int> activeTaxTypes = new HashSet<int>(await tenant.Set<MasterTdsDutyTaxType>().AsNoTracking()
                        .Where(row => requestedTaxTypes.Contains(row.TdsDutyTaxTypeId) && row.Status == 1)
                        .Select(row => row.TdsDutyTaxTypeId)
                        .ToListAsync(cancellationToken));
                    for (int i = 0; i < rows.Count; i++)
                    {
                        int taxTypeId = rows[i].TdsTaxTypeId;
                        if (!activeTaxTypes.Contains(taxTypeId) && !currentTaxTypes.Contains(taxTypeId))
                        {
                            return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSPolicy.TDS_TAX_TYPE_NOT_EXISTS));
                        }
                    }

                    // One policy per deductee type + nature of payment + applicable from date.
                    int currentId = isNew ? 0 : request.TdsPolicyId;
                    bool duplicate = await tenant.Set<MasterTdsPolicy>().AnyAsync(
                        row => row.TdsDeducteeTypeId == deducteeTypeId
                            && row.TdsNaturePaymentId == naturePayId
                            && row.ApplicableFrom >= applicableFrom
                            && row.ApplicableFrom < applicableTo
                            && row.TdsPolicyId != currentId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.TDS.TDSPolicy.TDS_DATE_DUPLICATE));
                    }

                    await using (IDbContextTransaction transaction = await tenant.Database.BeginTransactionAsync(cancellationToken))
                    {
                        MasterTdsPolicy policy;
                        if (existing == null)
                        {
                            policy = new MasterTdsPolicy
                            {
                                TdsDeducteeTypeId = request.DeducteeTypeId,
                                TdsNaturePaymentId = request.NaturePayId,
                                ApplicableFrom = applicableFrom
                            };
                            tenant.Set<MasterTdsPolicy>().Add(policy);
                        }
                        else
                        {
                            policy = existing;
                            policy.TdsDeducteeTypeId = request.DeducteeTypeId;
                            policy.TdsNaturePaymentId = request.NaturePayId;
                            policy.ApplicableFrom = applicableFrom;
                            List<MasterTdsTaxRate> oldRates = await tenant.Set<MasterTdsTaxRate>()
                                .Where(row => row.TdsPolicyId == policy.TdsPolicyId)
                                .ToListAsync(cancellationToken);
                            tenant.Set<MasterTdsTaxRate>().RemoveRange(oldRates);
                        }

                        await tenant.SaveChangesAsync(cancellationToken);

                        for (int i = 0; i < rows.Count; i++)
                        {
                            tenant.Set<MasterTdsTaxRate>().Add(new MasterTdsTaxRate
                            {
                                TdsPolicyId = policy.TdsPolicyId,
                                TdsTaxTypeId = rows[i].TdsTaxTypeId,
                                TdsRate = rows[i].TdsRate,
                                TdsExemptionLimit = rows[i].TdsExemptionLimit
                            });
                        }

                        await tenant.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_SAVE_SUCCESS), policy.TdsPolicyId);
                    }
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<TdsPolicyActionResult> DeleteTdsPolicyDetails(HeadOfficeConnection office, int tdsPolicyId, CancellationToken cancellationToken)
        {
            if (tdsPolicyId <= 0)
            {
                return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterTdsPolicy? existing = await tenant.Set<MasterTdsPolicy>().FirstOrDefaultAsync(
                        row => row.TdsPolicyId == tdsPolicyId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_NOT_FOUND));
                    }

                    await using (IDbContextTransaction transaction = await tenant.Database.BeginTransactionAsync(cancellationToken))
                    {
                        List<MasterTdsTaxRate> rates = await tenant.Set<MasterTdsTaxRate>()
                            .Where(row => row.TdsPolicyId == tdsPolicyId)
                            .ToListAsync(cancellationToken);
                        tenant.Set<MasterTdsTaxRate>().RemoveRange(rates);
                        tenant.Set<MasterTdsPolicy>().Remove(existing);
                        await tenant.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_DELETE_SUCCESS), tdsPolicyId);
                    }
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
    }
}
