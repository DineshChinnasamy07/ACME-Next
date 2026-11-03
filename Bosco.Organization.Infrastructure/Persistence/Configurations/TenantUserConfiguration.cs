/*  Class Name      : TenantUserConfiguration.cs
 *  Purpose         : Map TenantUser to public.user_info (HO tenant)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using Bosco.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Organization.Infrastructure.Persistence.Configurations
{
    public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
    {
        public void Configure(EntityTypeBuilder<TenantUser> builder)
        {
            builder.ToTable("user_info");
            builder.HasKey(user => user.UserId);

            builder.Property(user => user.UserId).HasColumnName("USER_ID").ValueGeneratedOnAdd();
            builder.Property(user => user.FirstName).HasColumnName("FIRSTNAME");
            builder.Property(user => user.LastName).HasColumnName("LASTNAME");
            builder.Property(user => user.UserName).HasColumnName("USER_NAME");
            builder.Property(user => user.Password).HasColumnName("PASSWORD");
            builder.Property(user => user.EmailId).HasColumnName("EMAIL_ID");
            builder.Property(user => user.ContactNo).HasColumnName("CONTACT_NO");
            builder.Property(user => user.RoleId).HasColumnName("ROLE_ID");
            builder.Property(user => user.Status).HasColumnName("STATUS");
            builder.Property(user => user.PasswordStatus).HasColumnName("PASSWORD_STATUS");
            builder.Property(user => user.UserType).HasColumnName("USER_TYPE");
            builder.Property(user => user.HeadOfficeCode).HasColumnName("HEAD_OFFICE_CODE");
            builder.Property(user => user.BranchCode).HasColumnName("BRANCH_CODE");
            builder.Property(user => user.CreatedBy).HasColumnName("CreatedBy");
        }
    }
}
