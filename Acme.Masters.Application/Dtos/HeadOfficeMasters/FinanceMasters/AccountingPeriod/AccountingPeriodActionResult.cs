/*  Class Name      : AccountingPeriodActionResult.cs
 *  Purpose         : Success text for Accounting Period actions
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public class AccountingPeriodActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int AccYearId { get; set; }
    }
}
