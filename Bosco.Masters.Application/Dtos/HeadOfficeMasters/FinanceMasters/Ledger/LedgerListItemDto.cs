/*  Class Name      : LedgerListItemDto.cs
 *  Purpose         : Ledger grid row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Ledger
{
    public class LedgerListItemDto
    {
        public int LedgerId { get; set; }

        public int GroupId { get; set; }

        public string LedgerCode { get; set; } = string.Empty;

        public string LedgerName { get; set; } = string.Empty;

        public string Group { get; set; } = string.Empty;

        public string LedgerType { get; set; } = string.Empty;

        public string LedgerSubType { get; set; } = string.Empty;

        public int BankAccountId { get; set; }

        public int NatureId { get; set; }

        public int AccessFlag { get; set; }

        public int BudgetGroupId { get; set; }

        public int BudgetSubGroupId { get; set; }
    }
}
