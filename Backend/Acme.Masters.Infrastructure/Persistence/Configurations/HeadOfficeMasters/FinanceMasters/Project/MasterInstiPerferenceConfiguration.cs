/*  Class Name      : MasterInstiPerferenceConfiguration.cs
 *  Purpose         : Map MasterInstiPerference to public.master_insti_perference
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 *  Modified on     : 24-Nov-2026 - Legal Entity columns mapped (Statutory Masters)
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.Project
{
    public class MasterInstiPerferenceConfiguration : IEntityTypeConfiguration<MasterInstiPerference>
    {
        public void Configure(EntityTypeBuilder<MasterInstiPerference> builder)
        {
            builder.ToTable("master_insti_perference");
            builder.HasKey(row => row.CustomerId);
            builder.Property(row => row.CustomerId).HasColumnName("CUSTOMERID").ValueGeneratedOnAdd();
            builder.Property(row => row.SocietyName).HasColumnName("SOCIETYNAME").HasMaxLength(100);
            builder.Property(row => row.ContactPerson).HasColumnName("CONTACTPERSON").HasMaxLength(100);
            builder.Property(row => row.Address).HasColumnName("ADDRESS").HasMaxLength(500);
            builder.Property(row => row.Place).HasColumnName("PLACE").HasMaxLength(100);
            builder.Property(row => row.State).HasColumnName("STATE").HasMaxLength(100);
            builder.Property(row => row.Country).HasColumnName("COUNTRY").HasMaxLength(100);
            builder.Property(row => row.Pincode).HasColumnName("PINCODE").HasMaxLength(20);
            builder.Property(row => row.Phone).HasColumnName("PHONE").HasMaxLength(20);
            builder.Property(row => row.Fax).HasColumnName("FAX").HasMaxLength(20);
            builder.Property(row => row.Email).HasColumnName("EMAIL").HasMaxLength(100);
            builder.Property(row => row.Url).HasColumnName("URL").HasMaxLength(100);
            builder.Property(row => row.RegNo).HasColumnName("REGNO").HasMaxLength(100);
            builder.Property(row => row.RegDate).HasColumnName("REGDATE");
            builder.Property(row => row.PermissionNo).HasColumnName("PERMISSIONNO").HasMaxLength(50);
            builder.Property(row => row.PermissionDate).HasColumnName("PERMISSIONDATE");
            builder.Property(row => row.A12No).HasColumnName("A12NO").HasMaxLength(100);
            builder.Property(row => row.PanNo).HasColumnName("PANNO").HasMaxLength(50);
            builder.Property(row => row.GirNo).HasColumnName("GIRNO").HasMaxLength(50);
            builder.Property(row => row.TanNo).HasColumnName("TANNO").HasMaxLength(50);
            builder.Property(row => row.FcriNo).HasColumnName("FCRINO").HasMaxLength(30);
            builder.Property(row => row.FcriRegDate).HasColumnName("FCRIREGDATE");
            builder.Property(row => row.EightyGNo).HasColumnName("EIGHTYGNO").HasMaxLength(20);
            builder.Property(row => row.EightyGNoRegDate).HasColumnName("EIGHTY_GNO_REG_DATE");
            builder.Property(row => row.AssociationNature).HasColumnName("ASSOCIATIONNATURE").HasMaxLength(35);
            builder.Property(row => row.OtherAssociationNature).HasColumnName("OTHER_ASSOCIATION_NATURE").HasMaxLength(100);
            builder.Property(row => row.Denomination).HasColumnName("DENOMINATION");
            builder.Property(row => row.OtherDenomination).HasColumnName("OTHER_DENOMINATION").HasMaxLength(100);
            builder.Property(row => row.IsFoundation).HasColumnName("IS_FOUNDATION");
            builder.Property(row => row.PrincipalActivity).HasColumnName("PRINCIPAL_ACTIVITY").HasMaxLength(100);
        }
    }
}
