/*  Class Name      : AccountingPeriodSaveRequest.cs
 *  Purpose         : SaveAccountingPeriodDetails request body
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public class AccountingPeriodSaveRequest
    {
        public int AccPeriodId { get; set; }

        public DateTime YearFrom { get; set; }

        public DateTime YearTo { get; set; }
    }
}
