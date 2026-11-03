/*  Class Name      : RightsUserConfiguration.cs
 *  Purpose         : Map RightsUser to public.rights_user
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Auth.Infrastructure.Persistence.Configurations
{
    public class RightsUserConfiguration : IEntityTypeConfiguration<RightsUser>
    {
        public void Configure(EntityTypeBuilder<RightsUser> builder)
        {
            builder.ToTable("rights_user");
            builder.HasKey(row => row.RightsId);
            builder.Property(row => row.RightsId).HasColumnName("RIGHTS_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.ModuleCode).HasColumnName("MODULE_CODE");
            builder.Property(row => row.ActivityCode).HasColumnName("ACTIVITY_CODE");
            builder.Property(row => row.RoleId).HasColumnName("ROLE_ID");
            builder.Property(row => row.Allow).HasColumnName("ALLOW");
        }
    }
}
