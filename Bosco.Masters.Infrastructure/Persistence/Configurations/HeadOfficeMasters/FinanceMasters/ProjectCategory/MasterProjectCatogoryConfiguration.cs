/*  Class Name      : MasterProjectCatogoryConfiguration.cs
 *  Purpose         : Map MasterProjectCatogory to public.master_project_catogory
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class MasterProjectCatogoryConfiguration : IEntityTypeConfiguration<MasterProjectCatogory>
    {
        public void Configure(EntityTypeBuilder<MasterProjectCatogory> builder)
        {
            builder.ToTable("master_project_catogory");
            builder.HasKey(row => row.ProjectCatogoryId);
            builder.Property(row => row.ProjectCatogoryId).HasColumnName("PROJECT_CATOGORY_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.ProjectCatogoryName).HasColumnName("PROJECT_CATOGORY_NAME").HasMaxLength(30);
            builder.Property(row => row.ProjectCatogoryGroupId).HasColumnName("PROJECT_CATOGORY_GROUP_ID");
            builder.Property(row => row.ProjectCatogoryItrgroupId).HasColumnName("PROJECT_CATOGORY_ITRGROUP_ID");
        }
    }
}
