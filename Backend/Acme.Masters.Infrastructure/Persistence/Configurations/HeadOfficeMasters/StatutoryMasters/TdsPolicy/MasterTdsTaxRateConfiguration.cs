/*  Class Name      : MasterTdsTaxRateConfiguration.cs
 *  Purpose         : Map MasterTdsTaxRate to public.tds_tax_rate
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class MasterTdsTaxRateConfiguration : IEntityTypeConfiguration<MasterTdsTaxRate>
    {
        public void Configure(EntityTypeBuilder<MasterTdsTaxRate> builder)
        {
            builder.ToTable("tds_tax_rate");
            builder.HasKey(row => row.TaxRateId);
            builder.Property(row => row.TaxRateId).HasColumnName("TAX_RATE_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.TdsPolicyId).HasColumnName("TDS_POLICY_ID");
            builder.Property(row => row.TdsRate).HasColumnName("TDS_RATE").HasPrecision(10, 2);
            builder.Property(row => row.TdsExemptionLimit).HasColumnName("TDS_EXEMPTION_LIMIT").HasPrecision(10, 2);
            builder.Property(row => row.TdsTaxTypeId).HasColumnName("TDS_TAX_TYPE_ID");
        }
    }
}
