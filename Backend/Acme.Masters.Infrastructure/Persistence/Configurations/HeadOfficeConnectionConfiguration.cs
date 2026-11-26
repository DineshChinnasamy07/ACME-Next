/*  Class Name      : HeadOfficeConnectionConfiguration.cs
 *  Purpose         : Map HeadOfficeConnection to public.head_office (portal)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Masters.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations
{
    public class HeadOfficeConnectionConfiguration : IEntityTypeConfiguration<HeadOfficeConnection>
    {
        public void Configure(EntityTypeBuilder<HeadOfficeConnection> builder)
        {
            builder.ToTable("head_office");
            builder.HasKey(row => row.HeadOfficeId);

            builder.Property(row => row.HeadOfficeId).HasColumnName("HEAD_OFFICE_ID");
            builder.Property(row => row.HeadOfficeCode).HasColumnName("HEAD_OFFICE_CODE");
            builder.Property(row => row.HostName).HasColumnName("HOST_NAME");
            builder.Property(row => row.DbName).HasColumnName("DB_NAME");
            builder.Property(row => row.UserName).HasColumnName("USERNAME");
            builder.Property(row => row.Password).HasColumnName("PASSWORD");
        }
    }
}
