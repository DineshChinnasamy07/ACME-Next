/*  Class Name      : BranchOfficeConfiguration.cs
 *  Purpose         : Map BranchOffice to public.branch_office
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using Bosco.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Organization.Infrastructure.Persistence.Configurations
{
    public class BranchOfficeConfiguration : IEntityTypeConfiguration<BranchOffice>
    {
        public void Configure(EntityTypeBuilder<BranchOffice> builder)
        {
            builder.ToTable("branch_office");
            builder.HasKey(row => row.BranchOfficeId);

            builder.Property(row => row.BranchOfficeId)
                .HasColumnName("BRANCH_OFFICE_ID")
                .ValueGeneratedOnAdd();

            builder.Property(row => row.BranchOfficeCode).HasColumnName("BRANCH_OFFICE_CODE");
            builder.Property(row => row.BranchOfficeName).HasColumnName("BRANCH_OFFICE_NAME");
            builder.Property(row => row.HeadOfficeCode).HasColumnName("HEAD_OFFICE_CODE");
            builder.Property(row => row.BranchPartCode).HasColumnName("BRANCH_PART_CODE");
            builder.Property(row => row.DeploymentType).HasColumnName("DEPLOYMENT_TYPE");
            builder.Property(row => row.Status).HasColumnName("STATUS");
            builder.Property(row => row.IsSubBranch).HasColumnName("IS_SUBBRANCH");
            builder.Property(row => row.Address).HasColumnName("ADDRESS");
            builder.Property(row => row.CountryId).HasColumnName("COUNTRY_ID");
            builder.Property(row => row.StateId).HasColumnName("STATE_ID");
            builder.Property(row => row.Pincode).HasColumnName("PINCODE");
            builder.Property(row => row.PhoneNo).HasColumnName("PHONE_NO");
            builder.Property(row => row.MobileNo).HasColumnName("MOBILE_NO");
            builder.Property(row => row.CountryCode).HasColumnName("COUNTRY_CODE");
            builder.Property(row => row.BranchEmail).HasColumnName("BRANCH_EMAIL_ID");
            builder.Property(row => row.City).HasColumnName("CITY");
            builder.Property(row => row.UserCreatedStatus).HasColumnName("USER_CREATED_STATUS");
            builder.Property(row => row.BranchKeyCode).HasColumnName("BRANCH_KEY_CODE");
            builder.Property(row => row.PersonIncharge).HasColumnName("INCHARGE_NAME");
            builder.Property(row => row.ThirdPartyCode).HasColumnName("THIRDPARTY_CODE");
            builder.Property(row => row.CreatedDate).HasColumnName("CREATED_DATE");
            builder.Property(row => row.CreatedBy).HasColumnName("CREATED_BY");
            builder.Property(row => row.ModifiedDate).HasColumnName("MODIFIED_DATE");
            builder.Property(row => row.ModifiedBy).HasColumnName("MODIFIED_BY");
        }
    }
}
