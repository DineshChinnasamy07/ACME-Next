/*  Class Name      : MasterLedgerGroupConfiguration.cs
 *  Purpose         : Map MasterLedgerGroup to public.master_ledger_group
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class MasterLedgerGroupConfiguration : IEntityTypeConfiguration<MasterLedgerGroup>
    {
        public void Configure(EntityTypeBuilder<MasterLedgerGroup> builder)
        {
            builder.ToTable("master_ledger_group");
            builder.HasKey(row => row.GroupId);

            builder.Property(row => row.GroupId).HasColumnName("GROUP_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.GroupCode).HasColumnName("GROUP_CODE").HasMaxLength(10);
            builder.Property(row => row.LedgerGroup).HasColumnName("LEDGER_GROUP").HasMaxLength(100);
            builder.Property(row => row.ParentGroupId).HasColumnName("PARENT_GROUP_ID");
            builder.Property(row => row.NatureId).HasColumnName("NATURE_ID");
            builder.Property(row => row.MainGroupId).HasColumnName("MAIN_GROUP_ID");
            builder.Property(row => row.ImageId).HasColumnName("IMAGE_ID");
            builder.Property(row => row.AccessFlag).HasColumnName("ACCESS_FLAG");
            builder.Property(row => row.SortOrder).HasColumnName("SORT_ORDER");
        }
    }
}
