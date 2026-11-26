/*  Class Name      : ExecutiveLegalEntityConfiguration.cs
 *  Purpose         : Map ExecutiveLegalEntity to public.executive_legal_entity
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies
{
    public class ExecutiveLegalEntityConfiguration : IEntityTypeConfiguration<ExecutiveLegalEntity>
    {
        public void Configure(EntityTypeBuilder<ExecutiveLegalEntity> builder)
        {
            builder.ToTable("executive_legal_entity");
            builder.HasKey(row => row.ExeLegalId);
            builder.Property(row => row.ExeLegalId).HasColumnName("EXE_LEGAL_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.CustomerId).HasColumnName("CUSTOMERID");
            builder.Property(row => row.ExecutiveId).HasColumnName("EXECUTIVE_ID");
        }
    }
}
