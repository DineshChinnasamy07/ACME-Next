/*  Class Name      : OrganizationDbContext.cs
 *  Purpose         : EF Core context for Organization portal Office tables
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bosco.Organization.Infrastructure.Persistence
{
    public class OrganizationDbContext : DbContext 
    {
        public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : base(options)
        {
        }
        public DbSet<HeadOffice> HeadOffices { get; set; } = null!;

        public DbSet<HeadOfficeType> HeadOfficeTypes { get; set; } = null!;

        public DbSet<Country> Countries { get; set; } = null!;

        public DbSet<State> States { get; set; } = null!;

        public DbSet<BranchOffice> BranchOffices { get; set; } = null!;

        public DbSet<TenantUser> TenantUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDbContext).Assembly);
        }
    }
}
