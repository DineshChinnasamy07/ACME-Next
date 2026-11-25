/*  Class Name      : LedgerGroupSaveRequest.cs
 *  Purpose         : SaveLedgerGroupDetails request body
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class LedgerGroupSaveRequest
    {
        public int GroupId { get; set; }

        public string? Abbrevation { get; set; }

        public string? Group { get; set; }

        public int ParentGroupId { get; set; }
    }
}
