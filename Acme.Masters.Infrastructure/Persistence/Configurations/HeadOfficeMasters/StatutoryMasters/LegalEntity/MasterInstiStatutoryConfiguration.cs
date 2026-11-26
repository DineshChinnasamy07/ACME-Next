/*  Class Name      : MasterInstiStatutoryConfiguration.cs
 *  Purpose         : Map MasterInstiStatutory to public.master_insti_statutory
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class MasterInstiStatutoryConfiguration : IEntityTypeConfiguration<MasterInstiStatutory>
    {
        public void Configure(EntityTypeBuilder<MasterInstiStatutory> builder)
        {
            builder.ToTable("master_insti_statutory");
            builder.HasKey(row => row.CustomerId);
            builder.Property(row => row.CustomerId).HasColumnName("CUSTOMERID").ValueGeneratedNever();
            builder.Property(row => row.EntityType).HasColumnName("ENTITY_TYPE");
            builder.Property(row => row.TrustDocNo).HasColumnName("TRUST_DOC_NO").HasMaxLength(100);
            builder.Property(row => row.BranchIncharge).HasColumnName("BRANCH_INCHARGE").HasMaxLength(100);
            builder.Property(row => row.PanStatus).HasColumnName("PAN_STATUS");
            builder.Property(row => row.PanName).HasColumnName("PAN_NAME").HasMaxLength(100);
            builder.Property(row => row.PanRegDate).HasColumnName("PAN_REG_DATE");
            builder.Property(row => row.DarpanStatus).HasColumnName("DARPAN_STATUS");
            builder.Property(row => row.DarpanId).HasColumnName("DARPAN_ID").HasMaxLength(30);
            builder.Property(row => row.DarpanRegDate).HasColumnName("DARPAN_REG_DATE");
            builder.Property(row => row.DarpanName).HasColumnName("DARPAN_NAME").HasMaxLength(100);
            builder.Property(row => row.A12Status).HasColumnName("A12_STATUS");
            builder.Property(row => row.A12ValidFrom).HasColumnName("A12_VALID_FROM");
            builder.Property(row => row.A12ValidTo).HasColumnName("A12_VALID_TO");
            builder.Property(row => row.A12Name).HasColumnName("A12_NAME").HasMaxLength(100);
            builder.Property(row => row.EightyGStatus).HasColumnName("EIGHTYG_STATUS");
            builder.Property(row => row.EightyGValidFrom).HasColumnName("EIGHTYG_VALID_FROM");
            builder.Property(row => row.EightyGValidTo).HasColumnName("EIGHTYG_VALID_TO");
            builder.Property(row => row.EightyGName).HasColumnName("EIGHTYG_NAME").HasMaxLength(100);
            builder.Property(row => row.FcraStatus).HasColumnName("FCRA_STATUS");
            builder.Property(row => row.FcraValidFrom).HasColumnName("FCRA_VALID_FROM");
            builder.Property(row => row.FcraValidTo).HasColumnName("FCRA_VALID_TO");
        }
    }
}
