/*  Class Name      : MappingBranchLocationConfiguration.cs
 *  Purpose         : Map MappingBranchLocation to public.branch_location
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapProjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.Mapping.MapProjects
{
    public class MappingBranchLocationConfiguration : IEntityTypeConfiguration<MappingBranchLocation>
    {
        public void Configure(EntityTypeBuilder<MappingBranchLocation> builder)
        {
            builder.ToTable("branch_location");
            builder.HasKey(row => row.LocationId);
            builder.Property(row => row.LocationId).HasColumnName("LOCATION_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.BranchId).HasColumnName("BRANCH_ID");
            builder.Property(row => row.LocationName).HasColumnName("LOCATION_NAME").HasMaxLength(200);
        }
    }
}
