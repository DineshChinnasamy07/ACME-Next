/*  Class Name      : HeadOfficeConfiguration.cs
 *  Purpose         : Map HeadOffice to public.head_office
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Auth.Infrastructure.Persistence.Configurations
{
    public class HeadOfficeConfiguration : IEntityTypeConfiguration<HeadOffice>
    {
        public void Configure(EntityTypeBuilder<HeadOffice> builder)
        {
            builder.ToTable("head_office");
            builder.HasKey(office => office.HeadOfficeId);
            builder.Property(office => office.HeadOfficeId).HasColumnName("HEAD_OFFICE_ID");
            builder.Property(office => office.HeadOfficeCode).HasColumnName("HEAD_OFFICE_CODE");
            builder.Property(office => office.HeadOfficeName).HasColumnName("HEAD_OFFICE_NAME");
            builder.Property(office => office.HostName).HasColumnName("HOST_NAME");
            builder.Property(office => office.DbName).HasColumnName("DB_NAME");
            builder.Property(office => office.UserName).HasColumnName("USERNAME");
            builder.Property(office => office.Password).HasColumnName("PASSWORD");
        }
    }
}
