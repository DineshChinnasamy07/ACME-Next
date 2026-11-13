/*  Class Name      : BranchLocationRepository.cs
 *  Purpose         : branch_location list, get, save, delete in the Head Office tenant database
 *  Author          : Dinesh C
 *  Created on      : 13-Nov-2026
 */
using Bosco.Common;
using Bosco.Organization.Application.Abstractions.Repositories;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;
using Bosco.Organization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Organization.Infrastructure.Repositories 
{
    public class BranchLocationRepository : IBranchLocationRepository 
    {
        private readonly IConfiguration _configuration;

        public BranchLocationRepository(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        #region Helpers
        private static BranchLocationActionResult Ok(string message,int locationId) 
        {
            return new BranchLocationActionResult { Success = true, Message = message, LocationId = locationId };
        }

        private static BranchLocationActionResult Fail(string message)
        {
            return new BranchLocationActionResult { Success = false, Message = message };
        }

        private static bool IsPrimary(BranchLocation row)
        {
            return row.BranchId == 0 || string.Equals(row.LocationName, "Primary", StringComparison.OrdinalIgnoreCase);
        }

        private static BranchLocationListItemDto ToItem(BranchLocation row, Dictionary<int, string> branchNames) 
        {
            string branch;
            if (!branchNames.TryGetValue(row.BranchId,out branch!)) 
            {
                branch = string.Empty;
            }

            return new BranchLocationListItemDto
            {
                LocationId = row.LocationId,
                BranchId = row.BranchId,
                Location = row.LocationName ?? string.Empty,
                Branch = branch,
                IsPrimary = IsPrimary(row)
            };
        }

        private OrganizationDbContext OpenTenant(HeadOffice office) 
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
            DbContextOptionsBuilder<OrganizationDbContext> builder = new DbContextOptionsBuilder<OrganizationDbContext>();
            builder.UseNpgsql(connectionString);
            return new OrganizationDbContext(builder.Options);
        }

        private static async Task<Dictionary<int, string>> BranchNames(OrganizationDbContext  tenant, CancellationToken cancellationToken) 
        {
            var branches = await tenant.BranchOffices
                .AsNoTracking()
                .Select(branch => new { branch.BranchOfficeId, branch.BranchOfficeName })
                .ToListAsync(cancellationToken);
            return branches.ToDictionary(branch => branch.BranchOfficeId, branch => branch.BranchOfficeName ?? string.Empty);
        }
        #endregion

        #region  Implementation of IBranchLocationRepository
        public async Task<IReadOnlyList<BranchLocationListItemDto>> FetchBranchLocationAll(HeadOffice office, CancellationToken cancellationToken)
        {
            await using (OrganizationDbContext tenant = OpenTenant(office))
            {
                List<BranchLocation> rows = await tenant.BranchLocations
                    .AsNoTracking()
                    .OrderBy(row => row.LocationName)
                    .ToListAsync(cancellationToken);
                Dictionary<int, string> names = await BranchNames(tenant, cancellationToken);
                return rows.Select(row => ToItem(row, names)).ToList();
            }
        }

        public async Task<BranchLocationListItemDto?> FillBranchLocationDetails(HeadOffice office, int locationId, CancellationToken cancellationToken)
        {
            await using (OrganizationDbContext tenant = OpenTenant(office)) 
            {
                BranchLocation? row = await tenant.BranchLocations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.LocationId == locationId, cancellationToken);
                if (row == null) 
                {
                    return null;
                }
                Dictionary<int, string> names = await BranchNames(tenant, cancellationToken);
                return ToItem(row, names);
            }
        }

        public async Task<IReadOnlyList<BranchOptionDto>> FetchBranch(HeadOffice office, CancellationToken cancellationToken)
        {
            await using (OrganizationDbContext tenant = OpenTenant(office))
            {
                return await tenant.BranchOffices
                    .AsNoTracking()
                    .Where(branch => branch.Status == 2 && branch.IsSubBranch != 1)
                    .OrderBy(branch => branch.BranchOfficeName)
                    .Select(branch => new BranchOptionDto
                    {
                        BranchOfficeId = branch.BranchOfficeId,
                        BranchOfficeCode = branch.BranchOfficeCode,
                        BranchOfficeName = branch.BranchOfficeName

                    }).ToListAsync(cancellationToken);
            }
        }

        public async Task<BranchLocationActionResult> SaveBranchLocationDetails(HeadOffice office, BranchLocationSaveRequest request, CancellationToken cancellationToken)
        {
            string name = (request.LocationName ?? string.Empty).Trim();
            if (request.BranchId <= 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationBranchRequired));
            }
            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationNameRequired));
            }
            if (name.Length > 200)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationNameMax));
            }
            try
            {
                await using (OrganizationDbContext tenant = OpenTenant(office))
                {
                    bool branchOk = await tenant.BranchOffices.AnyAsync(
                        branch => branch.BranchOfficeId == request.BranchId && branch.Status == 2 && branch.IsSubBranch != 1,cancellationToken);
                    if (!branchOk) 
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationBranchRequired));
                    }

                    string lower = name.ToLowerInvariant();
                    bool duplicate = await tenant.BranchLocations.AnyAsync(
                        row => row.LocationName.ToLower() == lower && row.LocationId != request.LocationId,
                        cancellationToken);
                    if (duplicate) 
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationNameDuplicate));
                    }
                    if (request.LocationId <= 0)
                    {
                        BranchLocation row = new BranchLocation { BranchId = request.BranchId, LocationName = name };
                        tenant.BranchLocations.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationSaved), row.LocationId);
                    }
                    BranchLocation? existing = await tenant.BranchLocations.FirstOrDefaultAsync(
                        row => row.LocationId == request.LocationId,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationNotFound));
                    }
                    if (IsPrimary(existing))
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationPrimaryProtected));
                    }
                    existing.BranchId = request.BranchId;
                    existing.LocationName = name;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationSaved), existing.LocationId);

                }
            }
            catch (Exception ex)
            {

                return Fail(ex.Message);
            }
        }

        public async Task<BranchLocationActionResult> DeleteLocation(HeadOffice office,int locationId, CancellationToken cancellationToken)
        {
            try
            {
                await using (OrganizationDbContext tenant = OpenTenant(office))
                {
                   BranchLocation? existing = await tenant.BranchLocations.FirstOrDefaultAsync(
                       row => row.LocationId == locationId,
                       cancellationToken);
                    if (existing == null)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationNotFound));
                    }
                    if (IsPrimary(existing))
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationPrimaryProtected));
                    }
                    tenant.BranchLocations.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationDeleted), locationId);

                }
            }
            catch (DbUpdateException)
            {
                // project_branch still points at this location.
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchLocationInUse));
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        #endregion
    }

}
