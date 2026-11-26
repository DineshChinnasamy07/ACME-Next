/*  Class Name      : MasterTdsSectionConfiguration.cs
 *  Purpose         : Map MasterTdsSection to public.tds_section
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.TdsSection
{
    public class MasterTdsSectionConfiguration : IEntityTypeConfiguration<MasterTdsSection>
    {
        public void Configure(EntityTypeBuilder<MasterTdsSection> builder)
        {
            builder.ToTable("tds_section");
            builder.HasKey(row => row.TdsSectionId);
            builder.Property(row => row.TdsSectionId).HasColumnName("TDS_SECTION_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.Code).HasColumnName("CODE").HasMaxLength(45);
            builder.Property(row => row.SectionName).HasColumnName("SECTION_NAME").HasMaxLength(100);
            builder.Property(row => row.Status).HasColumnName("STATUS");
        }
    }
}
