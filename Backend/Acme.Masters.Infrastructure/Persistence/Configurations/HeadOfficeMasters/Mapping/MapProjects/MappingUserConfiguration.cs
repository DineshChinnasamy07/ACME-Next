/*  Class Name      : MappingUserConfiguration.cs
 *  Purpose         : Map MappingUser to public.user_info
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapProjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.Mapping.MapProjects
{
    public class MappingUserConfiguration : IEntityTypeConfiguration<MappingUser>
    {
        public void Configure(EntityTypeBuilder<MappingUser> builder)
        {
            builder.ToTable("user_info");
            builder.HasKey(row => row.UserId);
            builder.Property(row => row.UserId).HasColumnName("USER_ID").ValueGeneratedNever();
            builder.Property(row => row.FirstName).HasColumnName("FIRSTNAME");
            builder.Property(row => row.LastName).HasColumnName("LASTNAME");
            builder.Property(row => row.UserName).HasColumnName("USER_NAME");
            builder.Property(row => row.BranchCode).HasColumnName("BRANCH_CODE");
            builder.Property(row => row.Status).HasColumnName("STATUS");
            builder.Property(row => row.UserType).HasColumnName("USER_TYPE");
        }
    }
}
