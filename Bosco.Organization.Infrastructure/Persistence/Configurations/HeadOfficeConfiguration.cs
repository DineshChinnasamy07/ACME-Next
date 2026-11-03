/*  Class Name      : HeadOfficeConfiguration.cs
 *  Purpose         : Map HeadOffice to public.head_office
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Organization.Infrastructure.Persistence.Configurations
{
    public class HeadOfficeConfiguration : IEntityTypeConfiguration<HeadOffice>
    {
        public void Configure(EntityTypeBuilder<HeadOffice> builder)
        {
            builder.ToTable("head_office");
            builder.HasKey(office => office.HeadOfficeId);

            builder.Property(office => office.HeadOfficeId)
                .HasColumnName("HEAD_OFFICE_ID")
                .ValueGeneratedOnAdd();

            builder.Property(office => office.HeadOfficeCode).HasColumnName("HEAD_OFFICE_CODE");
            builder.Property(office => office.HeadOfficeName).HasColumnName("HEAD_OFFICE_NAME");
            builder.Property(office => office.TypeId).HasColumnName("TYPE_ID");
            builder.Property(office => office.BelongsTo).HasColumnName("BELONGSTO");
            builder.Property(office => office.Designation).HasColumnName("DESIGNATION");
            builder.Property(office => office.OrgMailId).HasColumnName("ORG_MAIL_ID");
            builder.Property(office => office.InchargeName).HasColumnName("INCHARGE_NAME");
            builder.Property(office => office.Address).HasColumnName("ADDRESS");
            builder.Property(office => office.CreatedDate).HasColumnName("CREATED_DATE");
            builder.Property(office => office.CreatedBy).HasColumnName("CREATED_BY");
            builder.Property(office => office.ModifiedDate).HasColumnName("MODIFIED_DATE");
            builder.Property(office => office.ModifiedBy).HasColumnName("MODIFIED_BY");
            builder.Property(office => office.Status).HasColumnName("STATUS");
            builder.Property(office => office.City).HasColumnName("CITY");
            builder.Property(office => office.HostName).HasColumnName("HOST_NAME");
            builder.Property(office => office.DbName).HasColumnName("DB_NAME");
            builder.Property(office => office.UserName).HasColumnName("USERNAME");
            builder.Property(office => office.Password).HasColumnName("PASSWORD");
            builder.Property(office => office.UserCreatedStatus).HasColumnName("USER_CREATED_STATUS");
            builder.Property(office => office.CountryId).HasColumnName("COUNTRY_ID");
            builder.Property(office => office.StateId).HasColumnName("STATE_ID");
            builder.Property(office => office.Pincode).HasColumnName("PINCODE");
            builder.Property(office => office.SrInchargeName).HasColumnName("SR_INCHARGE_NAME");
            builder.Property(office => office.SrMobileNo).HasColumnName("SR_MOBILE_NO");
            builder.Property(office => office.SrPhoneNo).HasColumnName("SR_PHONE_NO");
            builder.Property(office => office.SrMailId).HasColumnName("SR_MAIL_ID");
            builder.Property(office => office.MobileNo).HasColumnName("MOBILE_NO");
            builder.Property(office => office.PhoneNo).HasColumnName("PHONE_NO");
            builder.Property(office => office.CountryCode).HasColumnName("COUNTRY_CODE");
            builder.Property(office => office.AccountingYearType).HasColumnName("ACCOUNTING_YEAR_TYPE");
        }
    }
}