/*  Class Name      : MasterExecutiveCommitteeConfiguration.cs
 *  Purpose         : Map MasterExecutiveCommittee to public.master_executive_committee
 *                    (IMAGES, STATE text and BRANCH_ID are not used by the HO screen; DB defaults apply)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.GoverningBodies
{
    public class MasterExecutiveCommitteeConfiguration : IEntityTypeConfiguration<MasterExecutiveCommittee>
    {
        public void Configure(EntityTypeBuilder<MasterExecutiveCommittee> builder)
        {
            builder.ToTable("master_executive_committee");
            builder.HasKey(row => row.ExecutiveId);
            builder.Property(row => row.ExecutiveId).HasColumnName("EXECUTIVE_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.Executive).HasColumnName("EXECUTIVE").HasMaxLength(50).IsRequired();
            builder.Property(row => row.Name).HasColumnName("NAME").HasMaxLength(50);
            builder.Property(row => row.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            builder.Property(row => row.Religion).HasColumnName("RELIGION").HasMaxLength(30);
            builder.Property(row => row.Role).HasColumnName("ROLE").HasMaxLength(30);
            builder.Property(row => row.Nationality).HasColumnName("NATIONALITY").HasMaxLength(25).IsRequired();
            builder.Property(row => row.Occupation).HasColumnName("OCCUPATION").HasMaxLength(25);
            builder.Property(row => row.Association).HasColumnName("ASSOCIATION").HasMaxLength(50);
            builder.Property(row => row.OfficeBearer).HasColumnName("OFFICE_BEARER").HasMaxLength(50);
            builder.Property(row => row.Place).HasColumnName("PLACE").HasMaxLength(30);
            builder.Property(row => row.CountryId).HasColumnName("COUNTRY_ID");
            builder.Property(row => row.StateId).HasColumnName("STATE_ID");
            builder.Property(row => row.Address).HasColumnName("ADDRESS").HasMaxLength(150);
            builder.Property(row => row.PinCode).HasColumnName("PIN_CODE").HasMaxLength(10);
            builder.Property(row => row.PanSsn).HasColumnName("PAN_SSN").HasMaxLength(10);
            builder.Property(row => row.Phone).HasColumnName("PHONE").HasMaxLength(20);
            builder.Property(row => row.Fax).HasColumnName("FAX").HasMaxLength(20);
            builder.Property(row => row.Email).HasColumnName("EMAIL").HasMaxLength(30);
            builder.Property(row => row.Url).HasColumnName("URL").HasMaxLength(30);
            builder.Property(row => row.DateOfAppointment).HasColumnName("DATE_OF_APPOINTMENT");
            builder.Property(row => row.DateOfExit).HasColumnName("DATE_OF_EXIT");
            builder.Property(row => row.Notes).HasColumnName("NOTES").HasMaxLength(500);
        }
    }
}
