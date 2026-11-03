/*  Class Name      : StateConfiguration.cs
 *  Purpose         : Map State to public.state
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using Bosco.Organization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Organization.Infrastructure.Persistence.Configurations
{
    public class StateConfiguration : IEntityTypeConfiguration<State>
    {
        public void Configure(EntityTypeBuilder<State> builder)
        {
            builder.ToTable("state");
            builder.HasKey(row => row.StateId);
            builder.Property(row => row.StateId).HasColumnName("STATE_ID").ValueGeneratedOnAdd();

            builder.Property(row => row.StateName).HasColumnName("STATE");
            builder.Property(row => row.StateCode).HasColumnName("STATE_CODE");
            builder.Property(row => row.CountryId).HasColumnName("COUNTRY_ID");
        }
    }
}
