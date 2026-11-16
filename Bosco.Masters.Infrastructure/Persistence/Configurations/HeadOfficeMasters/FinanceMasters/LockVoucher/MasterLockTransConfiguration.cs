/*  Class Name      : MasterLockTransConfiguration.cs
 *  Purpose         : Map MasterLockTrans to public.master_lock_trans
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class MasterLockTransConfiguration : IEntityTypeConfiguration<MasterLockTrans>
    {
        public void Configure(EntityTypeBuilder<MasterLockTrans> builder)
        {
            builder.ToTable("master_lock_trans");
            builder.HasKey(row => row.LockTransId);
            builder.Property(row => row.LockTransId).HasColumnName("LOCK_TRANS_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.BranchId).HasColumnName("BRANCH_ID");
            builder.Property(row => row.ProjectId).HasColumnName("PROJECT_ID");
            builder.Property(row => row.DateFrom).HasColumnName("DATE_FROM");
            builder.Property(row => row.DateTo).HasColumnName("DATE_TO");
            builder.Property(row => row.Password).HasColumnName("PASSWORD").HasMaxLength(500);
            builder.Property(row => row.Reason).HasColumnName("REASON").HasMaxLength(500);
            builder.Property(row => row.PasswordHint).HasColumnName("PASSWORD_HINT").HasMaxLength(50);
            builder.Property(row => row.LockType).HasColumnName("LOCK_TYPE").HasMaxLength(45);
            builder.Property(row => row.LockByPortal).HasColumnName("LOCK_BY_PORTAL");
        }
    }
}
