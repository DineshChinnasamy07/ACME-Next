using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.Project
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public ProjectRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static ProjectActionResult Ok(string message, int id)
        {
            return new ProjectActionResult { Success = true, Message = message, ProjectId = id };
        }

        private static ProjectActionResult Fail(string message)
        {
            return new ProjectActionResult { Success = false, Message = message };
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

        public async Task<IReadOnlyList<ProjectListItemDto>> FetchProjectDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterProject> rows = await tenant.Projects.AsNoTracking()
                    .Where(row => row.DeleteFlag != 1)
                    .OrderBy(row => row.ProjectCode)
                    .ThenBy(row => row.Project)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> divisions = await tenant.Divisions.AsNoTracking()
                    .ToDictionaryAsync(row => row.DivisionId, row => row.Division, cancellationToken);
                Dictionary<int, string> categories = await tenant.ProjectCatogories.AsNoTracking()
                    .ToDictionaryAsync(row => row.ProjectCatogoryId, row => row.ProjectCatogoryName ?? string.Empty, cancellationToken);
                Dictionary<int, string> societies = await tenant.Societies.AsNoTracking()
                    .ToDictionaryAsync(row => row.CustomerId, row => row.SocietyName ?? string.Empty, cancellationToken);

                List<ProjectListItemDto> items = new List<ProjectListItemDto>(rows.Count);
                for (int i = 0; i < rows.Count; i++)
                {
                    MasterProject row = rows[i];
                    string division;
                    string category;
                    string society;
                    divisions.TryGetValue(row.DivisionId, out division!);
                    categories.TryGetValue(row.ProjectCategoryId, out category!);
                    societies.TryGetValue(row.CustomerId, out society!);
                    items.Add(new ProjectListItemDto
                    {
                        ProjectId = row.ProjectId,
                        ProjectCode = row.ProjectCode ?? string.Empty,
                        ProjectName = row.Project ?? string.Empty,
                        DivisionId = row.DivisionId,
                        Division = division ?? string.Empty,
                        ProjectCategoryId = row.ProjectCategoryId,
                        Category = category ?? string.Empty,
                        CustomerId = row.CustomerId,
                        Society = society ?? string.Empty,
                        DateStarted = row.DateStarted,
                        DateClosed = row.DateClosed,
                        Description = row.Description ?? string.Empty
                    });
                }

                return items;
            }
        }

        public async Task<MasterProject?> FillProjectDetails(HeadOfficeConnection office, int projectId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.Projects.AsNoTracking()
                    .FirstOrDefaultAsync(row => row.ProjectId == projectId && row.DeleteFlag != 1, cancellationToken);
            }
        }

        public async Task<IReadOnlyList<ProjectOptionDto>> FetchDivision(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.Divisions.AsNoTracking()
                    .Where(row => row.Division != null && row.Division != string.Empty)
                    .OrderBy(row => row.Division)
                    .Select(row => new ProjectOptionDto { Id = row.DivisionId, Name = row.Division })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<ProjectOptionDto>> FetchProjectCategories(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.ProjectCatogories.AsNoTracking()
                    .OrderBy(row => row.ProjectCatogoryName)
                    .Select(row => new ProjectOptionDto
                    {
                        Id = row.ProjectCatogoryId,
                        Name = row.ProjectCatogoryName ?? string.Empty
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<ProjectOptionDto>> FetchSocietyNames(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.Societies.AsNoTracking()
                    .OrderBy(row => row.SocietyName)
                    .Select(row => new ProjectOptionDto
                    {
                        Id = row.CustomerId,
                        Name = row.SocietyName ?? string.Empty
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<ProjectActionResult> SaveProjectDetails(HeadOfficeConnection office, ProjectSaveRequest request, CancellationToken cancellationToken)
        {
            string code = (request.ProjectCode ?? string.Empty).Trim();
            string name = (request.ProjectName ?? string.Empty).Trim();
            if (code.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_CODE_EMPTY));
            }

            if (code.Length > 15)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_CODE_MAX));
            }

            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_NAME_EMPTY));
            }

            if (name.Length > 100)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_NAME_MAX));
            }

            if (request.DivisionId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_DIVISION_EMPTY));
            }

            if (request.ProjectCategroyId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_CATEGORY_EMPTY));
            }

            DateTime? started = NormalizeDate(request.StartedOn);
            DateTime? closed = NormalizeDate(request.ClosedOn);
            if (started.HasValue && closed.HasValue && started.Value > closed.Value)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_DATE_VALIDATION));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    bool divisionOk = await tenant.Divisions.AnyAsync(row => row.DivisionId == request.DivisionId, cancellationToken);
                    if (!divisionOk)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_DIVISION_EMPTY));
                    }

                    bool categoryOk = await tenant.ProjectCatogories.AnyAsync(
                        row => row.ProjectCatogoryId == request.ProjectCategroyId, cancellationToken);
                    if (!categoryOk)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_CATEGORY_EMPTY));
                    }

                    string lower = name.ToLowerInvariant();
                    bool duplicate = await tenant.Projects.AnyAsync(
                        row => row.DeleteFlag != 1
                            && row.Project.ToLower() == lower
                            && row.ProjectId != request.ProjectId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_EXISTS));
                    }

                    if (request.ProjectId <= 0)
                    {
                        MasterProject row = new MasterProject
                        {
                            ProjectCode = code,
                            Project = name,
                            DivisionId = request.DivisionId,
                            ProjectCategoryId = request.ProjectCategroyId,
                            CustomerId = request.SocietyId,
                            AccountDate = NormalizeDate(request.AccountDate),
                            DateStarted = started,
                            DateClosed = closed,
                            ClosedBy = closed.HasValue ? 1 : 0,
                            Description = request.Description,
                            Notes = request.Notes,
                            DeleteFlag = 0
                        };
                        tenant.Projects.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_SUCCESS), row.ProjectId);
                    }

                    MasterProject? existing = await tenant.Projects.FirstOrDefaultAsync(
                        row => row.ProjectId == request.ProjectId && row.DeleteFlag != 1, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_NOT_FOUND));
                    }

                    existing.ProjectCode = code;
                    existing.Project = name;
                    existing.DivisionId = request.DivisionId;
                    existing.ProjectCategoryId = request.ProjectCategroyId;
                    existing.CustomerId = request.SocietyId;
                    existing.AccountDate = NormalizeDate(request.AccountDate);
                    existing.DateStarted = started;
                    existing.DateClosed = closed;
                    existing.ClosedBy = closed.HasValue ? 1 : 0;
                    existing.Description = request.Description;
                    existing.Notes = request.Notes;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_SUCCESS), existing.ProjectId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<ProjectActionResult> DeleteProjectDetails(HeadOfficeConnection office, int projectId, CancellationToken cancellationToken)
        {
            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterProject? existing = await tenant.Projects.FirstOrDefaultAsync(
                        row => row.ProjectId == projectId && row.DeleteFlag != 1, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_NOT_FOUND));
                    }

                    List<int> balanceRows = await tenant.Database
                        .SqlQueryRaw<int>(
                            "SELECT 1 AS \"Value\" FROM public.ledger_balance WHERE \"PROJECT_ID\" = {0} LIMIT 1",
                            projectId)
                        .ToListAsync(cancellationToken);
                    if (balanceRows.Count > 0)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_TRANSACTION_MADE));
                    }

                    existing.DeleteFlag = 1;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_DELETE_SUCCESS), projectId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
    }
}
