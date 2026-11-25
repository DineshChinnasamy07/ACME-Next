/*  Class Name      : MapGoverningBodiesRepository.cs
 *  Purpose         : Map Governing Bodies (EXECUTIVE_LEGAL_ENTITY) on the HO tenant database.
 *                    Ported from legacy GoverningMemberMapping.aspx.cs and ExecutiveMemberSystem.MapGoverningMembers:
 *                    the ticked members replace the legal entity's mapping in one transaction.
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies
{
    public class MapGoverningBodiesRepository : IMapGoverningBodiesRepository
    {
        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public MapGoverningBodiesRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static GoverningBodyMappingActionResult Ok(string message, int customerId, int mappedCount)
        {
            return new GoverningBodyMappingActionResult { Success = true, Message = message, CustomerId = customerId, MappedCount = mappedCount };
        }

        private static GoverningBodyMappingActionResult Fail(string message)
        {
            return new GoverningBodyMappingActionResult { Success = false, Message = message };
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
        /// Legal Entity combo (legacy LoadLegalEntity: SOCIETYNAME / CUSTOMERID ordered by society name).
        /// </summary>
        public async Task<IReadOnlyList<LegalEntityOptionDto>> LoadLegalEntityLookup(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.Set<MasterInstiPerference>().AsNoTracking()
                    .OrderBy(row => row.SocietyName)
                    .Select(row => new LegalEntityOptionDto
                    {
                        Id = row.CustomerId,
                        Name = row.SocietyName ?? string.Empty
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Legacy FetchGoverningMemberByLegalEntity: every governing body member with Selected = mapped to the
        /// legal entity; mapped rows first (legacy sort "SELECT DESC"), then by name. Null when the legal entity is missing.
        /// </summary>
        public async Task<IReadOnlyList<GoverningBodyMappingRowDto>?> FetchGoverningMembersByLegalEntity(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                bool exists = await tenant.Set<MasterInstiPerference>().AsNoTracking()
                    .AnyAsync(row => row.CustomerId == customerId, cancellationToken);
                if (!exists)
                {
                    return null;
                }

                List<int> mappedIds = await tenant.Set<ExecutiveLegalEntity>().AsNoTracking()
                    .Where(row => row.CustomerId == customerId)
                    .Select(row => row.ExecutiveId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                HashSet<int> mapped = new HashSet<int>(mappedIds);

                List<MasterExecutiveCommittee> members = await tenant.Set<MasterExecutiveCommittee>().AsNoTracking()
                    .OrderBy(row => row.Executive)
                    .ToListAsync(cancellationToken);

                List<GoverningBodyMappingRowDto> rows = new List<GoverningBodyMappingRowDto>(members.Count);
                for (int i = 0; i < members.Count; i++)
                {
                    MasterExecutiveCommittee member = members[i];
                    rows.Add(new GoverningBodyMappingRowDto
                    {
                        ExecutiveId = member.ExecutiveId,
                        Executive = member.Executive,
                        Role = member.Role ?? string.Empty,
                        DateOfAppointment = member.DateOfAppointment.HasValue ? member.DateOfAppointment.Value.Date : null,
                        DateOfExit = member.DateOfExit.HasValue ? member.DateOfExit.Value.Date : null,
                        Selected = mapped.Contains(member.ExecutiveId)
                    });
                }

                return rows
                    .OrderByDescending(row => row.Selected)
                    .ThenBy(row => row.Executive, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        /// <summary>
        /// Legacy MapGoverningMembers: delete the legal entity's mapping, then insert one row per ticked member,
        /// inside one transaction. An empty selection clears the mapping.
        /// </summary>
        public async Task<GoverningBodyMappingActionResult> MapGoverningMembers(HeadOfficeConnection office, GoverningBodyMappingSaveRequest request, CancellationToken cancellationToken)
        {
            List<int> executiveIds = (request.ExecutiveIds ?? new List<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    bool exists = await tenant.Set<MasterInstiPerference>().AsNoTracking()
                        .AnyAsync(row => row.CustomerId == request.CustomerId, cancellationToken);
                    if (!exists)
                    {
                        return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_NOT_FOUND));
                    }

                    if (executiveIds.Count > 0)
                    {
                        int found = await tenant.Set<MasterExecutiveCommittee>().AsNoTracking()
                            .CountAsync(row => executiveIds.Contains(row.ExecutiveId), cancellationToken);
                        if (found != executiveIds.Count)
                        {
                            return Fail(MessageRender.GetMessage(GoverningLegalMessages.MAP_GOVERNING_MEMBER_INVALID));
                        }
                    }

                    await using (IDbContextTransaction transaction = await tenant.Database.BeginTransactionAsync(cancellationToken))
                    {
                        List<ExecutiveLegalEntity> current = await tenant.Set<ExecutiveLegalEntity>()
                            .Where(row => row.CustomerId == request.CustomerId)
                            .ToListAsync(cancellationToken);
                        tenant.Set<ExecutiveLegalEntity>().RemoveRange(current);
                        await tenant.SaveChangesAsync(cancellationToken);

                        for (int i = 0; i < executiveIds.Count; i++)
                        {
                            tenant.Set<ExecutiveLegalEntity>().Add(new ExecutiveLegalEntity
                            {
                                CustomerId = request.CustomerId,
                                ExecutiveId = executiveIds[i]
                            });
                        }

                        await tenant.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }

                    return Ok(MessageRender.GetMessage(GoverningLegalMessages.MAP_GOVERNING_SAVE_SUCCESS), request.CustomerId, executiveIds.Count);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        /// <summary>
        /// Legacy UnmapLegalEntitytoGoverningMember: removes every governing body mapping of the legal entity.
        /// </summary>
        public async Task<GoverningBodyMappingActionResult> DeleteMappedGoverningMember(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken)
        {
            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    bool exists = await tenant.Set<MasterInstiPerference>().AsNoTracking()
                        .AnyAsync(row => row.CustomerId == customerId, cancellationToken);
                    if (!exists)
                    {
                        return Fail(MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_NOT_FOUND));
                    }

                    List<ExecutiveLegalEntity> current = await tenant.Set<ExecutiveLegalEntity>()
                        .Where(row => row.CustomerId == customerId)
                        .ToListAsync(cancellationToken);
                    tenant.Set<ExecutiveLegalEntity>().RemoveRange(current);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(GoverningLegalMessages.MAP_GOVERNING_UNMAP_SUCCESS), customerId, 0);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
    }
}
