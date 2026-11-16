using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Bosco.Masters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class ProjectCategoryRepository : IProjectCategoryRepository
    {
        private readonly MastersDbContext _portal;
        private readonly IConfiguration _configuration;

        public ProjectCategoryRepository(MastersDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        private static ProjectCategoryActionResult Ok(string message, int id)
        {
            return new ProjectCategoryActionResult { Success = true, Message = message, ProjectCatogoryId = id };
        }

        private static ProjectCategoryActionResult Fail(string message)
        {
            return new ProjectCategoryActionResult { Success = false, Message = message };
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

        public async Task<IReadOnlyList<ProjectCategoryListItemDto>> FetchProjectCatogoryDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                List<MasterProjectCatogory> rows = await tenant.ProjectCatogories.AsNoTracking()
                    .OrderBy(row => row.ProjectCatogoryName)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> groups = await tenant.ProjectCatogoryGroups.AsNoTracking()
                    .ToDictionaryAsync(row => row.ProjectCatogoryGroupId, row => row.ProjectCatogoryGroup, cancellationToken);
                Dictionary<int, string> itrs = await tenant.ProjectCatogoryItrgroups.AsNoTracking()
                    .ToDictionaryAsync(row => row.ProjectCatogoryItrgroupId, row => row.ProjectCatogoryItrgroup, cancellationToken);

                List<ProjectCategoryListItemDto> items = new List<ProjectCategoryListItemDto>(rows.Count);
                for (int i = 0; i < rows.Count; i++)
                {
                    MasterProjectCatogory row = rows[i];
                    int groupId = row.ProjectCatogoryGroupId ?? 0;
                    int itrId = row.ProjectCatogoryItrgroupId ?? 0;
                    string group;
                    string itr;
                    groups.TryGetValue(groupId, out group!);
                    itrs.TryGetValue(itrId, out itr!);
                    items.Add(new ProjectCategoryListItemDto
                    {
                        ProjectCatogoryId = row.ProjectCatogoryId,
                        ProjectCatogoryName = row.ProjectCatogoryName ?? string.Empty,
                        ProjectCatogoryGroupId = groupId,
                        Group = group ?? string.Empty,
                        ProjectCatogoryItrgroupId = itrId,
                        ItrGroup = itr ?? string.Empty
                    });
                }

                return items;
            }
        }

        public async Task<MasterProjectCatogory?> FillProjectCatogoryDetails(HeadOfficeConnection office, int projectCatogoryId, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.ProjectCatogories.AsNoTracking()
                    .FirstOrDefaultAsync(row => row.ProjectCatogoryId == projectCatogoryId, cancellationToken);
            }
        }

        public async Task<IReadOnlyList<ProjectCategoryOptionDto>> FetchCategoryGroups(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.ProjectCatogoryGroups.AsNoTracking()
                    .OrderBy(row => row.ProjectCatogoryGroup)
                    .Select(row => new ProjectCategoryOptionDto
                    {
                        Id = row.ProjectCatogoryGroupId,
                        Name = row.ProjectCatogoryGroup
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<ProjectCategoryOptionDto>> FetchITRGroupCategory(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            await using (MastersDbContext tenant = OpenTenant(office))
            {
                return await tenant.ProjectCatogoryItrgroups.AsNoTracking()
                    .OrderBy(row => row.ProjectCatogoryItrgroup)
                    .Select(row => new ProjectCategoryOptionDto
                    {
                        Id = row.ProjectCatogoryItrgroupId,
                        Name = row.ProjectCatogoryItrgroup
                    })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<ProjectCategoryActionResult> SaveProjectCatogoryDetails(HeadOfficeConnection office, ProjectCategorySaveRequest request, CancellationToken cancellationToken)
        {
            string name = (request.ProjectCatogoryName ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_EMPTY));
            }

            if (name.Length > 30)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_NAME_MAX));
            }

            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    string lower = name.ToLowerInvariant();
                    bool duplicate = await tenant.ProjectCatogories.AnyAsync(
                        row => (row.ProjectCatogoryName ?? string.Empty).ToLower() == lower
                            && row.ProjectCatogoryId != request.ProjectCatogoryId,
                        cancellationToken);
                    if (duplicate)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_AVAILABLE));
                    }

                    if (request.ProjectCatogoryId <= 0)
                    {
                        MasterProjectCatogory row = new MasterProjectCatogory
                        {
                            ProjectCatogoryName = name,
                            ProjectCatogoryGroupId = request.GeneralateCategoryId > 0 ? request.GeneralateCategoryId : (int?)null,
                            ProjectCatogoryItrgroupId = request.ITRGroupId > 0 ? request.ITRGroupId : 1
                        };
                        tenant.ProjectCatogories.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_SAVE_SUCCESS), row.ProjectCatogoryId);
                    }

                    MasterProjectCatogory? existing = await tenant.ProjectCatogories.FirstOrDefaultAsync(
                        row => row.ProjectCatogoryId == request.ProjectCatogoryId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_NOT_FOUND));
                    }

                    existing.ProjectCatogoryName = name;
                    existing.ProjectCatogoryGroupId = request.GeneralateCategoryId > 0 ? request.GeneralateCategoryId : (int?)null;
                    existing.ProjectCatogoryItrgroupId = request.ITRGroupId > 0 ? request.ITRGroupId : 1;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_SAVE_SUCCESS), existing.ProjectCatogoryId);
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<ProjectCategoryActionResult> DeleteProjectCatogoryDetails(HeadOfficeConnection office, int projectCatogoryId, CancellationToken cancellationToken)
        {
            try
            {
                await using (MastersDbContext tenant = OpenTenant(office))
                {
                    MasterProjectCatogory? existing = await tenant.ProjectCatogories.FirstOrDefaultAsync(
                        row => row.ProjectCatogoryId == projectCatogoryId, cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_NOT_FOUND));
                    }

                    bool inUse = await tenant.Projects.AnyAsync(
                        row => row.ProjectCategoryId == projectCatogoryId && row.DeleteFlag != 1,
                        cancellationToken);
                    if (inUse)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_DELETE_ASSOCIATION));
                    }

                    tenant.ProjectCatogories.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_DELETE_SUCCESS), projectCatogoryId);
                }
            }
            catch (DbUpdateException)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_DELETE_ASSOCIATION));
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
    }
}
