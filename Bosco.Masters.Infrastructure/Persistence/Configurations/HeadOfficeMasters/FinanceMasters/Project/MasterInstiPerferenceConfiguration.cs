/*  Class Name      : MasterInstiPerferenceConfiguration.cs
 *  Purpose         : Map MasterInstiPerference to public.master_insti_perference
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.Project
{
    public class MasterInstiPerferenceConfiguration : IEntityTypeConfiguration<MasterInstiPerference>
    {
        public void Configure(EntityTypeBuilder<MasterInstiPerference> builder)
        {
            builder.ToTable("master_insti_perference");
            builder.HasKey(row => row.CustomerId);
            builder.Property(row => row.CustomerId).HasColumnName("CUSTOMERID");
            builder.Property(row => row.SocietyName).HasColumnName("SOCIETYNAME").HasMaxLength(100);
        }
    }
}
