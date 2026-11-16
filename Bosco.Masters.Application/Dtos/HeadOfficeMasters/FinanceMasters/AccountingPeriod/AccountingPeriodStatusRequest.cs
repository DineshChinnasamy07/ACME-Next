/*  Class Name      : AccountingPeriodStatusRequest.cs
 *  Purpose         : Activate accounting period (STATUS=1) request body
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.AccountingPeriod
{
    public class AccountingPeriodStatusRequest
    {
        public int Status { get; set; }
    }
}
