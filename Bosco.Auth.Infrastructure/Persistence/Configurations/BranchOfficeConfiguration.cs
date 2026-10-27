/*  Class Name      : BranchOfficeConfiguration.cs
 *  Purpose         : Map BranchOffice to public.branch_office
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Auth.Infrastructure.Persistence.Configurations
{
    public class BranchOfficeConfiguration : IEntityTypeConfiguration<BranchOffice>
    {
        public void Configure(EntityTypeBuilder<BranchOffice> builder)
        {
            builder.ToTable("branch_office");
            builder.HasKey(branch => branch.BranchOfficeId);
            builder.Property(branch => branch.BranchOfficeId).HasColumnName("BRANCH_OFFICE_ID");
            builder.Property(branch => branch.BranchOfficeCode).HasColumnName("BRANCH_OFFICE_CODE");
            builder.Property(branch => branch.BranchOfficeName).HasColumnName("BRANCH_OFFICE_NAME");
        }
    }
}
