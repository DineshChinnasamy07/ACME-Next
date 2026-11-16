/*  Class Name      : MasterDivisionConfiguration.cs
 *  Purpose         : Map MasterDivision to public.master_division
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.Project
{
    public class MasterDivisionConfiguration : IEntityTypeConfiguration<MasterDivision>
    {
        public void Configure(EntityTypeBuilder<MasterDivision> builder)
        {
            builder.ToTable("master_division");
            builder.HasKey(row => row.DivisionId);
            builder.Property(row => row.DivisionId).HasColumnName("DIVISION_ID");
            builder.Property(row => row.Division).HasColumnName("DIVISION").HasMaxLength(45);
        }
    }
}
