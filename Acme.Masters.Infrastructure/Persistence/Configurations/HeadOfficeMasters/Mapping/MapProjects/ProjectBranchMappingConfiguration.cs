/*  Class Name      : ProjectBranchMappingConfiguration.cs
 *  Purpose         : Map ProjectBranchMapping to public.project_branch_mapping
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapProjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.Mapping.MapProjects
{
    public class ProjectBranchMappingConfiguration : IEntityTypeConfiguration<ProjectBranchMapping>
    {
        public void Configure(EntityTypeBuilder<ProjectBranchMapping> builder)
        {
            builder.ToTable("project_branch_mapping");
            builder.HasKey(row => row.ProjectBranchMapId);
            builder.Property(row => row.ProjectBranchMapId).HasColumnName("PROJECT_BRANCH_MAP_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.ProjectId).HasColumnName("PROJECT_ID");
            builder.Property(row => row.BranchId).HasColumnName("BRANCH_ID");
            builder.Property(row => row.LocationId).HasColumnName("LOCATION_ID");
            builder.Property(row => row.CustomerId).HasColumnName("CUSTOMERID");
            builder.Property(row => row.ProjectCategoryId).HasColumnName("PROJECT_CATEGORY_ID");
            builder.Property(row => row.DivisionId).HasColumnName("DIVISION_ID");
            builder.Property(row => row.DateStarted).HasColumnName("DATE_STARTED").HasColumnType("timestamp without time zone");
            builder.Property(row => row.DateClosed).HasColumnName("DATE_CLOSED").HasColumnType("timestamp without time zone");
            builder.Property(row => row.UserId).HasColumnName("USER_ID");
            builder.Property(row => row.CreatedDate).HasColumnName("CREATED_DATE").HasColumnType("timestamp without time zone");
            builder.Property(row => row.ModifiedDate).HasColumnName("MODIFIED_DATE").HasColumnType("timestamp without time zone");
        }
    }
}
