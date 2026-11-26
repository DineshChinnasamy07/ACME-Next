/*  Class Name      : MasterInstiTanConfiguration.cs
 *  Purpose         : Map MasterInstiTan to public.master_insti_tan
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class MasterInstiTanConfiguration : IEntityTypeConfiguration<MasterInstiTan>
    {
        public void Configure(EntityTypeBuilder<MasterInstiTan> builder)
        {
            builder.ToTable("master_insti_tan");
            builder.HasKey(row => row.TanId);
            builder.Property(row => row.TanId).HasColumnName("TAN_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.CustomerId).HasColumnName("CUSTOMERID");
            builder.Property(row => row.TanNo).HasColumnName("TAN_NO").HasMaxLength(10);
            builder.Property(row => row.TanRegDate).HasColumnName("TAN_REG_DATE");
            builder.Property(row => row.TanName).HasColumnName("TAN_NAME").HasMaxLength(100);
        }
    }
}
