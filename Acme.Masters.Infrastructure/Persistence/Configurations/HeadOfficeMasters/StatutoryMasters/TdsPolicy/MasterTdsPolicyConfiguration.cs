/*  Class Name      : MasterTdsPolicyConfiguration.cs
 *  Purpose         : Map MasterTdsPolicy to public.tds_policy
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class MasterTdsPolicyConfiguration : IEntityTypeConfiguration<MasterTdsPolicy>
    {
        public void Configure(EntityTypeBuilder<MasterTdsPolicy> builder)
        {
            builder.ToTable("tds_policy");
            builder.HasKey(row => row.TdsPolicyId);
            builder.Property(row => row.TdsPolicyId).HasColumnName("TDS_POLICY_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.TdsDeducteeTypeId).HasColumnName("TDS_DEDUCTEE_TYPE_ID");
            builder.Property(row => row.TdsNaturePaymentId).HasColumnName("TDS_NATURE_PAYMENT_ID");
            builder.Property(row => row.ApplicableFrom).HasColumnName("APPLICABLE_FROM");
        }
    }
}
