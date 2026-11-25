/*  Class Name      : LegalEntityRepository.cs
 *  Purpose         : Legal Entity (MASTER_INSTI_PERFERENCE) list / fill / save / delete on the HO tenant database.
 *                    Rules ported from legacy LegalEntityAdd.aspx.cs ValidateUserControl and LegalEntitySQL.
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using System.Text.RegularExpressions;
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntityRepository : ILegalEntityRepository
    {
        // Legacy association nature ids (Association enum): 0 Cultural .. 3 Religious .. 5 Others.
        private const int AssociationReligious = 3;
        private const int AssociationOthers = 5;
        private const int AssociationMax = 5;

        // Legacy denomination ids (Denomination enum): 1 Hindu .. 6 Others.
        private const int DenominationOthers = 6;

        private static readonly Regex EmailPattern = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public LegalEntityRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static LegalEntityActionResult Ok(string message, int id)
        {
            return new LegalEntityActionResult { Success = true, Message = message, CustomerId = id };
        }

        private static LegalEntityActionResult Fail(string message)
        {
            return new LegalEntityActionResult { Success = false, Message = message };
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

        /// <summary>
        /// Legacy LegalEntitySQL.FetchAll (HO login: no branch filter), ordered by society name.
        /// </summary>
        public async Task<IReadOnlyList<LegalEntityListItemDto>> FetchLegalEntity(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<LegalEntityListItemDto> rows = await tenant.Set<MasterInstiPerference>().AsNoTracking()
                    .OrderBy(row => row.SocietyName)
                    .Select(row => new LegalEntityListItemDto
                    {
                        CustomerId = row.CustomerId,
                        SocietyName = row.SocietyName ?? string.Empty,
                        RegNo = row.RegNo ?? string.Empty,
                        RegDate = row.RegDate,
                        Place = row.Place ?? string.Empty,
                        State = row.State ?? string.Empty,
                        Country = row.Country ?? string.Empty
                    })
                    .ToListAsync(cancellationToken);

                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i].RegDate = ReadDate(rows[i].RegDate);
                }

                return rows;
            }
        }

        public async Task<LegalEntityDetailDto?> FillLegalEntityDetails(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                MasterInstiPerference? row = await tenant.Set<MasterInstiPerference>().AsNoTracking()
                    .FirstOrDefaultAsync(item => item.CustomerId == customerId, cancellationToken);
                if (row == null)
                {
                    return null;
                }

                return new LegalEntityDetailDto
                {
                    CustomerId = row.CustomerId,
                    SocietyName = row.SocietyName,
                    ContactPerson = row.ContactPerson,
                    Address = row.Address,
                    Place = row.Place,
                    State = row.State,
                    Country = row.Country,
                    Pincode = row.Pincode,
                    Phone = row.Phone,
                    Fax = row.Fax,
                    Email = row.Email,
                    Url = row.Url,
                    RegNo = row.RegNo,
                    RegDate = ReadDate(row.RegDate),
                    PermissionNo = row.PermissionNo,
                    PermissionDate = ReadDate(row.PermissionDate),
                    A12No = row.A12No,
                    PanNo = row.PanNo,
                    GirNo = row.GirNo,
                    TanNo = row.TanNo,
                    FcriNo = row.FcriNo,
                    FcriRegDate = ReadDate(row.FcriRegDate),
                    EightyGNo = row.EightyGNo,
                    EightyGNoRegDate = ReadDate(row.EightyGNoRegDate),
                    AssociationNature = row.AssociationNature,
                    OtherAssociationNature = row.OtherAssociationNature,
                    Denomination = row.Denomination ?? 0,
                    OtherDenomination = row.OtherDenomination,
                    IsFoundation = row.IsFoundation == 1,
                    PrincipalActivity = row.PrincipalActivity
                };
            }
        }

        public async Task<LegalEntityActionResult> SaveLegalEntityDetails(HeadOfficeConnection office, LegalEntitySaveRequest request, CancellationToken cancellationToken)
        {
            string societyName = Clean(request.SocietyName);
            string country = Clean(request.Country);
            string state = Clean(request.State);
            string regNo = Clean(request.RegNo);
            string email = Clean(request.Email);

            // Legacy ValidateUserControl order: society, country, state, reg no, reg date, permission date,
            // association nature (+ others text), denomination when Religious (+ others text).
            if (societyName.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.SOCIETY_NAME_EMPTY));
            }

            if (country.Length == 0)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_COUNTRY_EMPTY));
            }

            if (state.Length == 0)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_STATE_EMPTY));
            }

            if (regNo.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.License.LEGAL_ENTITY_REGNO_EMPTY));
            }

            DateTime today = DateTime.Today;
            if (request.RegDate.HasValue && request.RegDate.Value.Date > today)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_REG_DATE_FUTURE));
            }

            if (request.PermissionDate.HasValue && request.PermissionDate.Value.Date > today)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_PERMISSION_DATE_FUTURE));
            }

            List<int> natures = ParseAssociationNature(request.AssociationNature);
            if (natures.Count == 0)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_ASSOCIATION_NATURE_EMPTY));
            }

            bool isOthers = natures.Contains(AssociationOthers);
            bool isReligious = natures.Contains(AssociationReligious);
            string otherAssociation = isOthers ? Clean(request.OtherAssociationNature) : string.Empty;
            if (isOthers && otherAssociation.Length == 0)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_ASSOCIATION_OTHER_EMPTY));
            }

            int denomination = isReligious ? request.Denomination : 0;
            if (isReligious && (denomination < 1 || denomination > DenominationOthers))
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_DENOMINATION_EMPTY));
            }

            string otherDenomination = denomination == DenominationOthers ? Clean(request.OtherDenomination) : string.Empty;
            if (denomination == DenominationOthers && otherDenomination.Length == 0)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_DENOMINATION_OTHER_EMPTY));
            }

            if (email.Length > 0 && !EmailPattern.IsMatch(email))
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_EMAIL_INVALID));
            }

            string contactPerson = Clean(request.ContactPerson);
            string address = Clean(request.Address);
            string place = Clean(request.Place);
            string pincode = Clean(request.Pincode);
            string phone = Clean(request.Phone);
            string fax = Clean(request.Fax);
            string url = Clean(request.Url);
            string permissionNo = Clean(request.PermissionNo);
            string a12No = Clean(request.A12No);
            string panNo = Clean(request.PanNo).ToUpperInvariant();
            string girNo = Clean(request.GirNo);
            string tanNo = Clean(request.TanNo).ToUpperInvariant();
            string fcriNo = Clean(request.FcriNo);
            string eightyGNo = Clean(request.EightyGNo);
            string principalActivity = Clean(request.PrincipalActivity);
            string associationNature = string.Join(",", natures);

            string? tooLong = CheckLength("Society Name", societyName, 100)
                ?? CheckLength("Contact Person", contactPerson, 100)
                ?? CheckLength("Address", address, 500)
                ?? CheckLength("Place", place, 100)
                ?? CheckLength("State/Province", state, 100)
                ?? CheckLength("Country", country, 100)
                ?? CheckLength("Pincode", pincode, 20)
                ?? CheckLength("Phone", phone, 20)
                ?? CheckLength("Fax", fax, 20)
                ?? CheckLength("Email", email, 100)
                ?? CheckLength("URL", url, 100)
                ?? CheckLength("Society/Reg.No", regNo, 100)
                ?? CheckLength("Prior Permission No", permissionNo, 50)
                ?? CheckLength("12A No", a12No, 100)
                ?? CheckLength("PAN No", panNo, 10)
                ?? CheckLength("GIR No", girNo, 50)
                ?? CheckLength("TAN No", tanNo, 10)
                ?? CheckLength("FCRA No", fcriNo, 30)
                ?? CheckLength("80G.No", eightyGNo, 20)
                ?? CheckLength("Association Nature Others", otherAssociation, 100)
                ?? CheckLength("Denomination Others", otherDenomination, 100)
                ?? CheckLength("Principal Activity", principalActivity, 100);
            if (tooLong != null)
            {
                return Fail(tooLong);
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    // SOCIETYNAME has a unique index (master_insti_perference_unique_societyname).
                    string lower = societyName.ToLowerInvariant();
                    bool duplicate = await tenant.Set<MasterInstiPerference>().AnyAsync(
                        row => (row.SocietyName ?? string.Empty).ToLower() == lower
                            && row.CustomerId != request.CustomerId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_EXISTS));
                    }

                    MasterInstiPerference? target;
                    if (request.CustomerId <= 0)
                    {
                        target = new MasterInstiPerference();
                        tenant.Set<MasterInstiPerference>().Add(target);
                    }
                    else
                    {
                        target = await tenant.Set<MasterInstiPerference>().FirstOrDefaultAsync(
                            row => row.CustomerId == request.CustomerId, cancellationToken);
                        if (target == null)
                        {
                            return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_NOT_FOUND));
                        }
                    }

                    target.SocietyName = societyName;
                    target.ContactPerson = contactPerson;
                    target.Address = address;
                    target.Place = place;
                    target.State = state;
                    target.Country = country;
                    target.Pincode = pincode;
                    target.Phone = phone;
                    target.Fax = fax;
                    target.Email = email;
                    target.Url = url;
                    target.RegNo = regNo;
                    target.RegDate = WriteDate(request.RegDate);
                    target.PermissionNo = permissionNo;
                    target.PermissionDate = WriteDate(request.PermissionDate);
                    target.A12No = a12No;
                    target.PanNo = panNo;
                    target.GirNo = girNo;
                    target.TanNo = tanNo;
                    target.FcriNo = fcriNo;
                    target.FcriRegDate = WriteDate(request.FcriRegDate);
                    target.EightyGNo = eightyGNo;
                    target.EightyGNoRegDate = WriteDate(request.EightyGNoRegDate);
                    target.AssociationNature = associationNature;
                    target.OtherAssociationNature = otherAssociation;
                    target.Denomination = denomination;
                    target.OtherDenomination = otherDenomination;
                    target.IsFoundation = request.IsFoundation ? 1 : 0;
                    target.PrincipalActivity = principalActivity;

                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_SAVE_SUCCESS), target.CustomerId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        /// <summary>
        /// A legal entity linked to projects (MASTER_PROJECT.CUSTOMERID, FK) or mapped to governing bodies
        /// (EXECUTIVE_LEGAL_ENTITY) cannot be deleted.
        /// </summary>
        public async Task<LegalEntityActionResult> DeleteLegalEntityDetails(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken)
        {
            if (customerId <= 0)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterInstiPerference? existing = await tenant.Set<MasterInstiPerference>().FirstOrDefaultAsync(
                        row => row.CustomerId == customerId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_NOT_FOUND));
                    }

                    bool usedByProject = await tenant.Set<MasterProject>().AsNoTracking()
                        .AnyAsync(row => row.CustomerId == customerId, cancellationToken);
                    if (usedByProject)
                    {
                        return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_USED_BY_PROJECT));
                    }

                    bool mapped = await tenant.Set<ExecutiveLegalEntity>().AsNoTracking()
                        .AnyAsync(row => row.CustomerId == customerId, cancellationToken);
                    if (mapped)
                    {
                        return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_MAPPED_TO_GOVERNING_BODY));
                    }

                    tenant.Set<MasterInstiPerference>().Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_DELETE_SUCCESS), customerId);
                }
            }
            catch (Exception)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_DELETE_FAILURE));
            }
        }

        private static string Clean(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static string? CheckLength(string caption, string value, int max)
        {
            if (value.Length <= max)
            {
                return null;
            }

            return MessageRender.GetMessage(GoverningLegalMessages.STATUTORY_FIELD_MAX, caption, max);
        }

        /// <summary>
        /// "0,3,5" -> distinct valid association nature ids (0..5) in ascending order, as the legacy page stored them.
        /// </summary>
        private static List<int> ParseAssociationNature(string? value)
        {
            List<int> natures = new List<int>();
            string[] parts = (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                int nature;
                if (int.TryParse(parts[i], out nature) && nature >= 0 && nature <= AssociationMax && !natures.Contains(nature))
                {
                    natures.Add(nature);
                }
            }

            natures.Sort();
            return natures;
        }

        // Tenant date columns are timestamp without time zone; EF maps DateTime as timestamptz, so write UTC-kind dates.
        private static DateTime? WriteDate(DateTime? value)
        {
            return value.HasValue ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc) : null;
        }

        private static DateTime? ReadDate(DateTime? value)
        {
            return value.HasValue ? value.Value.Date : null;
        }
    }
}
