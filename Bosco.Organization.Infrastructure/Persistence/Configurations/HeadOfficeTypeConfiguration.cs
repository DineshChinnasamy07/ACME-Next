/*  Class Name      : HeadOfficeTypeConfiguration.cs
 *  Purpose         : Map HeadOfficeType to public.head_office_type
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using Bosco.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Organization.Infrastructure.Persistence.Configurations
{
    public class HeadOfficeTypeConfiguration : IEntityTypeConfiguration<HeadOfficeType>
    {
        public void Configure(EntityTypeBuilder<HeadOfficeType> builder)
        {
            builder.ToTable("head_office_type");
            builder.HasKey(row => row.TypeId);

            builder.Property(row => row.TypeId)
                .HasColumnName("TYPE_ID")
                .ValueGeneratedOnAdd();

            builder.Property(row => row.Type)
                .HasColumnName("TYPE");
        }
    }
}
