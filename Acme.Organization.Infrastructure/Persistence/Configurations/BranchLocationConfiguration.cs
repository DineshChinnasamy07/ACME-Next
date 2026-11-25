using Acme.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Organization.Infrastructure.Persistence.Configurations
{
    public class BranchLocationConfiguration : IEntityTypeConfiguration<BranchLocation>
    {
        public void Configure(EntityTypeBuilder<BranchLocation> builder)
        {
            builder.ToTable("branch_location");
            builder.HasKey(row => row.LocationId);
            builder.Property(row => row.LocationId).HasColumnName("LOCATION_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.BranchId).HasColumnName("BRANCH_ID");
            builder.Property(row => row.LocationName).HasColumnName("LOCATION_NAME");
        }
    }
}