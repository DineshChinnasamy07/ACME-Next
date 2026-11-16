/*  Class Name      : MasterBranchOfficeConfiguration.cs
 *  Purpose         : Map MasterBranchOffice to public.branch_office
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.LockVoucher
{
    public class MasterBranchOfficeConfiguration : IEntityTypeConfiguration<MasterBranchOffice>
    {
        public void Configure(EntityTypeBuilder<MasterBranchOffice> builder)
        {
            builder.ToTable("branch_office");
            builder.HasKey(row => row.BranchOfficeId);
            builder.Property(row => row.BranchOfficeId).HasColumnName("BRANCH_OFFICE_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.BranchOfficeCode).HasColumnName("BRANCH_OFFICE_CODE").HasMaxLength(16);
            builder.Property(row => row.BranchOfficeName).HasColumnName("BRANCH_OFFICE_NAME").HasMaxLength(50);
            builder.Property(row => row.Status).HasColumnName("STATUS");
        }
    }
}
