/*  Class Name      : MasterTdsDutyTaxTypeConfiguration.cs
 *  Purpose         : Map MasterTdsDutyTaxType to public.tds_duty_taxtype
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.DutyTax
{
    public class MasterTdsDutyTaxTypeConfiguration : IEntityTypeConfiguration<MasterTdsDutyTaxType>
    {
        public void Configure(EntityTypeBuilder<MasterTdsDutyTaxType> builder)
        {
            builder.ToTable("tds_duty_taxtype");
            builder.HasKey(row => row.TdsDutyTaxTypeId);
            builder.Property(row => row.TdsDutyTaxTypeId).HasColumnName("TDS_DUTY_TAXTYPE_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.TaxTypeName).HasColumnName("TAX_TYPE_NAME").HasMaxLength(50);
            builder.Property(row => row.Status).HasColumnName("STATUS");
        }
    }
}
