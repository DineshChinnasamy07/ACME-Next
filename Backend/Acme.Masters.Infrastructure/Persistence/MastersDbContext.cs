/*  Class Name      : MastersDbContext.cs
 *  Purpose         : EF Core context for Masters (portal HO connection + HO tenant masters)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapBranchMasters;
using Microsoft.EntityFrameworkCore;

namespace Acme.Masters.Infrastructure.Persistence
{
    public class MastersDbContext : DbContext
    {
        public MastersDbContext(DbContextOptions<MastersDbContext> options) : base(options)
        {
        }

        public DbSet<HeadOfficeConnection> HeadOfficeConnections { get; set; } = null!;

        public DbSet<AccountingYear> AccountingYears { get; set; } = null!;

        public DbSet<MasterLedgerGroup> LedgerGroups { get; set; } = null!;

        public DbSet<MasterLedger> Ledgers { get; set; } = null!;

        public DbSet<MasterProjectCatogory> ProjectCatogories { get; set; } = null!;

        public DbSet<MasterProjectCatogoryGroup> ProjectCatogoryGroups { get; set; } = null!;

        public DbSet<MasterProjectCatogoryItrgroup> ProjectCatogoryItrgroups { get; set; } = null!;

        public DbSet<MasterProject> Projects { get; set; } = null!;

        public DbSet<MasterDivision> Divisions { get; set; } = null!;

        public DbSet<MasterInstiPerference> Societies { get; set; } = null!;

        public DbSet<MasterContributionHead> ContributionHeads { get; set; } = null!;

        public DbSet<CongregationLedger> CongregationLedgers { get; set; } = null!;

        public DbSet<CongregationLedgerMap> CongregationLedgerMaps { get; set; } = null!;

        public DbSet<MasterLockTrans> LockTrans { get; set; } = null!;

        public DbSet<MasterBranchOffice> BranchOffices { get; set; } = null!;

        public DbSet<ProjectBranch> ProjectBranches { get; set; } = null!;

        public DbSet<MasterBranchMap> MasterBranchMaps { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MastersDbContext).Assembly);
        }
    }
}
