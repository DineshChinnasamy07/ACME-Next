/*  Class Name      : MasterBranchMapConfiguration.cs
 *  Purpose         : Map MasterBranchMap to public.master_branch_map
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapBranchMasters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.Mapping.MapBranchMasters
{
    public class MasterBranchMapConfiguration : IEntityTypeConfiguration<MasterBranchMap>
    {
        public void Configure(EntityTypeBuilder<MasterBranchMap> builder)
        {
            builder.ToTable("master_branch_map");
            builder.HasKey(row => row.MasterBranchMapId);
            builder.Property(row => row.MasterBranchMapId).HasColumnName("MASTER_BRANCH_MAP_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.MasterType).HasColumnName("MASTER_TYPE");
            builder.Property(row => row.MasterId).HasColumnName("MASTER_ID");
            builder.Property(row => row.BranchId).HasColumnName("BRANCH_ID");
            builder.Property(row => row.CreatedDate).HasColumnName("CREATED_DATE").HasColumnType("timestamp without time zone");
        }
    }
}
