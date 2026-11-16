/*  Class Name      : MasterContributionHeadConfiguration.cs
 *  Purpose         : Map MasterContributionHead to public.master_contribution_head
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.FcPurpose
{
    public class MasterContributionHeadConfiguration : IEntityTypeConfiguration<MasterContributionHead>
    {
        public void Configure(EntityTypeBuilder<MasterContributionHead> builder)
        {
            builder.ToTable("master_contribution_head");
            builder.HasKey(row => row.ContributionId);
            builder.Property(row => row.ContributionId).HasColumnName("CONTRIBUTION_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.Code).HasColumnName("CODE").HasMaxLength(15);
            builder.Property(row => row.FcPurpose).HasColumnName("FC_PURPOSE").HasMaxLength(150);
        }
    }
}
