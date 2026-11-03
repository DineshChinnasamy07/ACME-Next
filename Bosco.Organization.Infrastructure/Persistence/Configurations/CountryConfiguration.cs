/*  Class Name      : CountryConfiguration.cs
 *  Purpose         : Map Country to public.country
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using Bosco.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Organization.Infrastructure.Persistence.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("country");
            builder.HasKey(row => row.CountryId);

            builder.Property(row => row.CountryId)
                .HasColumnName("COUNTRY_ID")
                .ValueGeneratedOnAdd();

            builder.Property(row => row.CountryCode)
                .HasColumnName("COUNTRY_CODE");

            builder.Property(row => row.CountryName)
                .HasColumnName("COUNTRY");
        }
    }
}
