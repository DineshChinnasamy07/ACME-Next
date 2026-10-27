/*  Class Name      : RoleConfiguration.cs
 *  Purpose         : Map Role to public.user_role
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Auth.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("user_role");
            builder.HasKey(role => role.UserRoleId);
            builder.Property(role => role.UserRoleId).HasColumnName("USERROLE_ID");
            builder.Property(role => role.UserRole).HasColumnName("USERROLE");
        }
    }
}
