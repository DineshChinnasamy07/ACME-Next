/*  Class Name      : MasterProjectCatogoryItrgroupConfiguration.cs
 *  Purpose         : Map MasterProjectCatogoryItrgroup to public.master_project_catogory_itrgroup
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class MasterProjectCatogoryItrgroupConfiguration : IEntityTypeConfiguration<MasterProjectCatogoryItrgroup>
    {
        public void Configure(EntityTypeBuilder<MasterProjectCatogoryItrgroup> builder)
        {
            builder.ToTable("master_project_catogory_itrgroup");
            builder.HasKey(row => row.ProjectCatogoryItrgroupId);
            builder.Property(row => row.ProjectCatogoryItrgroupId).HasColumnName("PROJECT_CATOGORY_ITRGROUP_ID");
            builder.Property(row => row.ProjectCatogoryItrgroup).HasColumnName("PROJECT_CATOGORY_ITRGROUP").HasMaxLength(45);
        }
    }
}
