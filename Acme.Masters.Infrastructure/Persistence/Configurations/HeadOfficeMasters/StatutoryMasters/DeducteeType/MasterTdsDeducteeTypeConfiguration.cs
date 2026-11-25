/*  Class Name      : MasterTdsDeducteeTypeConfiguration.cs
 *  Purpose         : Map MasterTdsDeducteeType to public.tds_deductee_type
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.DeducteeType
{
    public class MasterTdsDeducteeTypeConfiguration : IEntityTypeConfiguration<MasterTdsDeducteeType>
    {
        public void Configure(EntityTypeBuilder<MasterTdsDeducteeType> builder)
        {
            builder.ToTable("tds_deductee_type");
            builder.HasKey(row => row.DeducteeTypeId);
            builder.Property(row => row.DeducteeTypeId).HasColumnName("DEDUCTEE_TYPE_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.Name).HasColumnName("NAME").HasMaxLength(100);
            builder.Property(row => row.ResidentialStatus).HasColumnName("RESIDENTIAL_STATUS");
            builder.Property(row => row.DeducteeType).HasColumnName("DEDUCTEE_TYPE");
            builder.Property(row => row.Status).HasColumnName("STATUS");
        }
    }
}
