/*  Class Name      : RightsActivityConfiguration.cs
 *  Purpose         : Map RightsActivity to public.rights_activities
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Auth.Infrastructure.Persistence.Configurations
{
    public class RightsActivityConfiguration : IEntityTypeConfiguration<RightsActivity>
    {
        public void Configure(EntityTypeBuilder<RightsActivity> builder)
        {
            builder.ToTable("rights_activities");
            builder.HasKey(row => row.ActivityId);
            builder.Property(row => row.ActivityId).HasColumnName("ACTIVITY_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.ModuleCode).HasColumnName("MODULE_CODE");
            builder.Property(row => row.Module).HasColumnName("MODULE");
            builder.Property(row => row.ActivityCode).HasColumnName("ACTIVITY_CODE");
            builder.Property(row => row.Activity).HasColumnName("ACTIVITY");
            builder.Property(row => row.ModuleOrder).HasColumnName("MODULE_ORDER");
            builder.Property(row => row.ActivityOrder).HasColumnName("ACTIVITY_ORDER");
            builder.Property(row => row.Type).HasColumnName("TYPE");
            builder.Property(row => row.Accessibility).HasColumnName("ACCESSIBILITY");
        }
    }
}
