/*  Class Name      : GoverningBodiesRepository.cs
 *  Purpose         : Governing Bodies (MASTER_EXECUTIVE_COMMITTEE) list / fill / save / delete on the HO tenant database.
 *                    Rules ported from legacy GoverningMemberAdd.aspx.cs ValidGoverningMember and ExecutiveMemberSQL.
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using System.Text.RegularExpressions;
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.GoverningBodies
{
    public class GoverningBodiesRepository : IGoverningBodiesRepository
    {
        private static readonly Regex EmailPattern = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public GoverningBodiesRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static GoverningBodyActionResult Ok(string message, int id)
        {
            return new GoverningBodyActionResult { Success = true, Message = message, ExecutiveId = id };
        }

        private static GoverningBodyActionResult Fail(string message)
        {
            return new GoverningBodyActionResult { Success = false, Message = message };
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
        /// Legacy ExecutiveMemberSQL.FetchAll ordered by EXECUTIVE. Country / state names are resolved by the client
        /// from the portal country list (legacy combos were bound from the portal COUNTRY / STATE tables).
        /// </summary>
        public async Task<IReadOnlyList<GoverningBodyListItemDto>> FetchExecutiveMemberDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<GoverningBodyListItemDto> rows = await tenant.Set<MasterExecutiveCommittee>().AsNoTracking()
                    .OrderBy(row => row.Executive)
                    .Select(row => new GoverningBodyListItemDto
                    {
                        ExecutiveId = row.ExecutiveId,
                        Executive = row.Executive,
                        Name = row.Name ?? string.Empty,
                        Nationality = row.Nationality,
                        Occupation = row.Occupation ?? string.Empty,
                        Role = row.Role ?? string.Empty,
                        CountryId = row.CountryId ?? 0,
                        StateId = row.StateId ?? 0,
                        DateOfAppointment = row.DateOfAppointment,
                        DateOfExit = row.DateOfExit
                    })
                    .ToListAsync(cancellationToken);

                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i].DateOfAppointment = ReadDate(rows[i].DateOfAppointment);
                    rows[i].DateOfExit = ReadDate(rows[i].DateOfExit);
                }

                return rows;
            }
        }

        public async Task<GoverningBodyDetailDto?> FillExecutiveMemberDetails(HeadOfficeConnection office, int executiveId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                MasterExecutiveCommittee? row = await tenant.Set<MasterExecutiveCommittee>().AsNoTracking()
                    .FirstOrDefaultAsync(item => item.ExecutiveId == executiveId, cancellationToken);
                if (row == null)
                {
                    return null;
                }

                return new GoverningBodyDetailDto
                {
                    ExecutiveId = row.ExecutiveId,
                    Executive = row.Executive,
                    Name = row.Name,
                    DateOfBirth = ReadDate(row.DateOfBirth),
                    Religion = row.Religion,
                    Role = row.Role,
                    Nationality = row.Nationality,
                    Occupation = row.Occupation,
                    Association = row.Association,
                    OfficeBearer = row.OfficeBearer,
                    Place = row.Place,
                    CountryId = row.CountryId ?? 0,
                    StateId = row.StateId ?? 0,
                    Address = row.Address,
                    PinCode = row.PinCode,
                    PanSsn = row.PanSsn,
                    Phone = row.Phone,
                    Fax = row.Fax,
                    Email = row.Email,
                    Url = row.Url,
                    DateOfAppointment = ReadDate(row.DateOfAppointment),
                    DateOfExit = ReadDate(row.DateOfExit),
                    Notes = row.Notes
                };
            }
        }

        public async Task<GoverningBodyActionResult> SaveExecutiveMemberDetails(HeadOfficeConnection office, GoverningBodySaveRequest request, CancellationToken cancellationToken)
        {
            string executive = Clean(request.Executive);
            string nationality = Clean(request.Nationality);
            string email = Clean(request.Email);

            // Legacy ValidGoverningMember order: name, nationality, date checks, country, state, email.
            if (executive.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_NAME_EMPTY));
            }

            if (nationality.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_NAIONALITY_EMPTY));
            }

            DateTime? dateOfBirth = request.DateOfBirth.HasValue ? request.DateOfBirth.Value.Date : null;
            DateTime? dateOfJoin = request.DateOfAppointment.HasValue ? request.DateOfAppointment.Value.Date : null;
            DateTime? dateOfExit = request.DateOfExit.HasValue ? request.DateOfExit.Value.Date : null;
            if (dateOfBirth.HasValue && dateOfJoin.HasValue && dateOfJoin.Value < dateOfBirth.Value)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_JOIN_DOB));
            }

            if (dateOfBirth.HasValue && dateOfExit.HasValue && dateOfExit.Value < dateOfBirth.Value)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_DOB_EXIT));
            }

            if (dateOfJoin.HasValue && dateOfExit.HasValue && dateOfExit.Value < dateOfJoin.Value)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_EXIT_JOIN));
            }

            if (request.CountryId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_COUNTRY_EMPTY));
            }

            if (request.StateId <= 0)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.GOVERNING_BODY_STATE_EMPTY));
            }

            if (email.Length > 0 && !EmailPattern.IsMatch(email))
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_EMAIL_EMPTY));
            }

            string fatherName = Clean(request.Name);
            string religion = Clean(request.Religion);
            string role = Clean(request.Role);
            string occupation = Clean(request.Occupation);
            string association = Clean(request.Association);
            string officeBearer = Clean(request.OfficeBearer);
            string place = Clean(request.Place);
            string address = Clean(request.Address);
            string pinCode = Clean(request.PinCode);
            string panSsn = Clean(request.PanSsn).ToUpperInvariant();
            string phone = Clean(request.Phone);
            string fax = Clean(request.Fax);
            string url = Clean(request.Url);
            string notes = Clean(request.Notes);

            string? tooLong = CheckLength("Name", executive, 50)
                ?? CheckLength("Father/Husband's Name", fatherName, 50)
                ?? CheckLength("Religion", religion, 30)
                ?? CheckLength("Role", role, 30)
                ?? CheckLength("Nationality", nationality, 25)
                ?? CheckLength("Occupation", occupation, 25)
                ?? CheckLength("Office Held in Association", association, 50)
                ?? CheckLength("Relationship with Office Bearer", officeBearer, 50)
                ?? CheckLength("Place", place, 30)
                ?? CheckLength("Address", address, 150)
                ?? CheckLength("Pin/Zip Code", pinCode, 10)
                ?? CheckLength("PAN/UID #", panSsn, 10)
                ?? CheckLength("Contact No", phone, 20)
                ?? CheckLength("Fax", fax, 20)
                ?? CheckLength("Email", email, 30)
                ?? CheckLength("URL", url, 30)
                ?? CheckLength("Notes", notes, 500);
            if (tooLong != null)
            {
                return Fail(tooLong);
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterExecutiveCommittee? target;
                    if (request.ExecutiveId <= 0)
                    {
                        target = new MasterExecutiveCommittee();
                        tenant.Set<MasterExecutiveCommittee>().Add(target);
                    }
                    else
                    {
                        target = await tenant.Set<MasterExecutiveCommittee>().FirstOrDefaultAsync(
                            row => row.ExecutiveId == request.ExecutiveId, cancellationToken);
                        if (target == null)
                        {
                            return Fail(MessageRender.GetMessage(GoverningLegalMessages.GOVERNING_BODY_NOT_FOUND));
                        }
                    }

                    target.Executive = executive;
                    target.Name = fatherName;
                    target.DateOfBirth = WriteDate(dateOfBirth);
                    target.Religion = religion;
                    target.Role = role;
                    target.Nationality = nationality;
                    target.Occupation = occupation;
                    target.Association = association;
                    target.OfficeBearer = officeBearer;
                    target.Place = place;
                    target.CountryId = request.CountryId;
                    target.StateId = request.StateId;
                    target.Address = address;
                    target.PinCode = pinCode;
                    target.PanSsn = panSsn;
                    target.Phone = phone;
                    target.Fax = fax;
                    target.Email = email;
                    target.Url = url;
                    target.DateOfAppointment = WriteDate(dateOfJoin);
                    target.DateOfExit = WriteDate(dateOfExit);
                    target.Notes = notes;

                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_SAVE_SUCCESS), target.ExecutiveId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        /// <summary>
        /// A member mapped to a legal entity (EXECUTIVE_LEGAL_ENTITY) cannot be deleted; unmap it first.
        /// </summary>
        public async Task<GoverningBodyActionResult> DeleteExecutiveMemberDetails(HeadOfficeConnection office, int executiveId, CancellationToken cancellationToken)
        {
            if (executiveId <= 0)
            {
                return Fail(MessageRender.GetMessage(GoverningLegalMessages.GOVERNING_BODY_NOT_FOUND));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterExecutiveCommittee? existing = await tenant.Set<MasterExecutiveCommittee>().FirstOrDefaultAsync(
                        row => row.ExecutiveId == executiveId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(GoverningLegalMessages.GOVERNING_BODY_NOT_FOUND));
                    }

                    bool mapped = await tenant.Set<ExecutiveLegalEntity>().AsNoTracking()
                        .AnyAsync(row => row.ExecutiveId == executiveId, cancellationToken);
                    if (mapped)
                    {
                        return Fail(MessageRender.GetMessage(GoverningLegalMessages.GOVERNING_BODY_MAPPED_TO_LEGAL_ENTITY));
                    }

                    tenant.Set<MasterExecutiveCommittee>().Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_DELETE_SUCCESS), executiveId);
                }
            }
            catch (Exception)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ExecutiveMembers.EXECUTIVE_DELETE_FAILURE));
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
