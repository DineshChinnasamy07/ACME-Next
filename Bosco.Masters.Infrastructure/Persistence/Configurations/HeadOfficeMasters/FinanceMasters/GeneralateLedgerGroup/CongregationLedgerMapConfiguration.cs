/*  Class Name      : CongregationLedgerMapConfiguration.cs
 *  Purpose         : Map CongregationLedgerMap to public.congregation_ledger_map
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class CongregationLedgerMapConfiguration : IEntityTypeConfiguration<CongregationLedgerMap>
    {
        public void Configure(EntityTypeBuilder<CongregationLedgerMap> builder)
        {
            builder.ToTable("congregation_ledger_map");
            builder.HasKey(row => new { row.ConLedgerId, row.LedgerId, row.ProjectCatogoryGroupId });
            builder.Property(row => row.ConLedgerId).HasColumnName("CON_LEDGER_ID");
            builder.Property(row => row.LedgerId).HasColumnName("LEDGER_ID");
            builder.Property(row => row.ProjectCatogoryGroupId).HasColumnName("PROJECT_CATOGORY_GROUP_ID");
        }
    }
}
