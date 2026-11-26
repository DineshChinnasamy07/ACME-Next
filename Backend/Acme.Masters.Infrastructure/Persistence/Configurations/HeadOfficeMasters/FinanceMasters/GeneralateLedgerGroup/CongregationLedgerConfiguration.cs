/*  Class Name      : CongregationLedgerConfiguration.cs
 *  Purpose         : Map CongregationLedger to public.congregation_ledger
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class CongregationLedgerConfiguration : IEntityTypeConfiguration<CongregationLedger>
    {
        public void Configure(EntityTypeBuilder<CongregationLedger> builder)
        {
            builder.ToTable("congregation_ledger");
            builder.HasKey(row => row.ConLedgerId);
            builder.Property(row => row.ConLedgerId).HasColumnName("CON_LEDGER_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.ConLedgerCode).HasColumnName("CON_LEDGER_CODE").HasMaxLength(10);
            builder.Property(row => row.ConLedgerName).HasColumnName("CON_LEDGER_NAME").HasMaxLength(100);
            builder.Property(row => row.ConParentLedgerId).HasColumnName("CON_PARENT_LEDGER_ID");
            builder.Property(row => row.ConMainParentId).HasColumnName("CON_MAIN_PARENT_ID");
        }
    }
}
