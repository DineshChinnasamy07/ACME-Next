/*  Class Name      : AccountingYear.cs
 *  Purpose         : HO tenant ACCOUNTING_YEAR row for Accounting Period CRUD
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public class AccountingYear
    {
        public int AccYearId { get; set; }

        public DateTime YearFrom { get; set; }

        public DateTime YearTo { get; set; }

        public DateTime? BooksBeginningFrom { get; set; }

        public int Status { get; set; }

        public int IsFirstAccountingYear { get; set; }
    }
}
