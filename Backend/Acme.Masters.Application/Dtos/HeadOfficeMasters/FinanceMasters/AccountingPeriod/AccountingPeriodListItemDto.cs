/*  Class Name      : AccountingPeriodListItemDto.cs
 *  Purpose         : Accounting Period grid row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public class AccountingPeriodListItemDto
    {
        public int AccYearId { get; set; }

        public DateTime YearFrom { get; set; }

        public DateTime YearTo { get; set; }

        public int Status { get; set; }

        public string YearStatus { get; set; } = string.Empty;
    }
}
