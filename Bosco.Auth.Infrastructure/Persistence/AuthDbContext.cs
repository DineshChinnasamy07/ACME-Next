/*  Class Name      : AuthDbContext.cs
 *  Purpose         : EF Core context for portal and head office login tables
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bosco.Auth.Infrastructure.Persistence
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Role> Roles { get; set; } = null!;

        public DbSet<HeadOffice> HeadOffices { get; set; } = null!;

        public DbSet<BranchOffice> BranchOffices { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        }
    }
}
