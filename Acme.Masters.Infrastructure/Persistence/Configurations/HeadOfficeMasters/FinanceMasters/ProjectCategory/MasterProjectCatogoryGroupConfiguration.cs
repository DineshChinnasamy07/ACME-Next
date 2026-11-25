/*  Class Name      : MasterProjectCatogoryGroupConfiguration.cs
 *  Purpose         : Map MasterProjectCatogoryGroup to public.master_project_catogory_group
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class MasterProjectCatogoryGroupConfiguration : IEntityTypeConfiguration<MasterProjectCatogoryGroup>
    {
        public void Configure(EntityTypeBuilder<MasterProjectCatogoryGroup> builder)
        {
            builder.ToTable("master_project_catogory_group");
            builder.HasKey(row => row.ProjectCatogoryGroupId);
            builder.Property(row => row.ProjectCatogoryGroupId).HasColumnName("PROJECT_CATOGORY_GROUP_ID");
            builder.Property(row => row.ProjectCatogoryGroup).HasColumnName("PROJECT_CATOGORY_GROUP").HasMaxLength(45);
        }
    }
}
