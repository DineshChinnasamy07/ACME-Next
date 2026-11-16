/*  Class Name      : MasterProjectConfiguration.cs
 *  Purpose         : Map MasterProject to public.master_project
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.Project
{
    public class MasterProjectConfiguration : IEntityTypeConfiguration<MasterProject>
    {
        public void Configure(EntityTypeBuilder<MasterProject> builder)
        {
            builder.ToTable("master_project");
            builder.HasKey(row => row.ProjectId);
            builder.Property(row => row.ProjectId).HasColumnName("PROJECT_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.ProjectCode).HasColumnName("PROJECT_CODE").HasMaxLength(15);
            builder.Property(row => row.Project).HasColumnName("PROJECT").HasMaxLength(100);
            builder.Property(row => row.DivisionId).HasColumnName("DIVISION_ID");
            builder.Property(row => row.AccountDate).HasColumnName("ACCOUNT_DATE");
            builder.Property(row => row.DateStarted).HasColumnName("DATE_STARTED");
            builder.Property(row => row.DateClosed).HasColumnName("DATE_CLOSED");
            builder.Property(row => row.ClosedBy).HasColumnName("CLOSED_BY");
            builder.Property(row => row.Description).HasColumnName("DESCRIPTION").HasMaxLength(200);
            builder.Property(row => row.Notes).HasColumnName("NOTES").HasMaxLength(500);
            builder.Property(row => row.ProjectCategoryId).HasColumnName("PROJECT_CATEGORY_ID");
            builder.Property(row => row.DeleteFlag).HasColumnName("DELETE_FLAG");
            builder.Property(row => row.CustomerId).HasColumnName("CUSTOMERID");
            builder.Property(row => row.ContributionId).HasColumnName("CONTRIBUTION_ID");
        }
    }
}
