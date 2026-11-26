/*  Class Name      : MappingBranchUserConfiguration.cs
 *  Purpose         : Map MappingBranchUser to public.branch_user
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapProjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.Mapping.MapProjects
{
    public class MappingBranchUserConfiguration : IEntityTypeConfiguration<MappingBranchUser>
    {
        public void Configure(EntityTypeBuilder<MappingBranchUser> builder)
        {
            builder.ToTable("branch_user");
            builder.HasKey(row => row.BranchUserId);
            builder.Property(row => row.BranchUserId).HasColumnName("BRANCH_USER_ID").ValueGeneratedNever();
            builder.Property(row => row.BranchId).HasColumnName("BRANCH_ID");
            builder.Property(row => row.UserId).HasColumnName("USER_ID");
            builder.Property(row => row.UserType).HasColumnName("USER_TYPE");
        }
    }
}
