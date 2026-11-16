/*  Class Name      : AccountingYearConfiguration.cs
 *  Purpose         : Map AccountingYear to public.accounting_year
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bosco.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public class AccountingYearConfiguration : IEntityTypeConfiguration<AccountingYear>
    {
        public void Configure(EntityTypeBuilder<AccountingYear> builder)
        {
            builder.ToTable("accounting_year");
            builder.HasKey(row => row.AccYearId);

            builder.Property(row => row.AccYearId).HasColumnName("ACC_YEAR_ID");
            builder.Property(row => row.YearFrom).HasColumnName("YEAR_FROM");
            builder.Property(row => row.YearTo).HasColumnName("YEAR_TO");
            builder.Property(row => row.BooksBeginningFrom).HasColumnName("BOOKS_BEGINNING_FROM");
            builder.Property(row => row.Status).HasColumnName("STATUS");
            builder.Property(row => row.IsFirstAccountingYear).HasColumnName("IS_FIRST_ACCOUNTING_YEAR");
        }
    }
}
