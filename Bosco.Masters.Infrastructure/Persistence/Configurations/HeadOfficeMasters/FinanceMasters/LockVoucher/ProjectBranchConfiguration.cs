/*  Class Name      : ProjectBranchConfiguration.cs
 *  Purpose         : Map ProjectBranch to public.project_branch
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class ProjectBranchConfiguration : IEntityTypeConfiguration<ProjectBranch>
    {
        public void Configure(EntityTypeBuilder<ProjectBranch> builder)
        {
            builder.ToTable("project_branch");
            builder.HasKey(row => new { row.ProjectId, row.BranchId, row.LocationId });
            builder.Property(row => row.ProjectId).HasColumnName("PROJECT_ID");
            builder.Property(row => row.BranchId).HasColumnName("BRANCH_ID");
            builder.Property(row => row.LocationId).HasColumnName("LOCATION_ID");
        }
    }
}
