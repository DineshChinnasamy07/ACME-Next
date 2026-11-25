/*  Class Name      : MasterTdsNaturePaymentConfiguration.cs
 *  Purpose         : Map MasterTdsNaturePayment to public.tds_nature_payment
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public class MasterTdsNaturePaymentConfiguration : IEntityTypeConfiguration<MasterTdsNaturePayment>
    {
        public void Configure(EntityTypeBuilder<MasterTdsNaturePayment> builder)
        {
            builder.ToTable("tds_nature_payment");
            builder.HasKey(row => row.NaturePayId);
            builder.Property(row => row.NaturePayId).HasColumnName("NATURE_PAY_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.Name).HasColumnName("NAME").HasMaxLength(150);
            builder.Property(row => row.Section).HasColumnName("SECTION").HasMaxLength(7);
            builder.Property(row => row.SectionName).HasColumnName("SECTION_NAME").HasMaxLength(100);
            builder.Property(row => row.PaymentCode).HasColumnName("PAYMENT_CODE").HasMaxLength(7);
            builder.Property(row => row.Description).HasColumnName("DESCRIPTION").HasMaxLength(400);
            builder.Property(row => row.TdsSectionId).HasColumnName("TDS_SECTION_ID");
            builder.Property(row => row.Status).HasColumnName("STATUS");
        }
    }
}
