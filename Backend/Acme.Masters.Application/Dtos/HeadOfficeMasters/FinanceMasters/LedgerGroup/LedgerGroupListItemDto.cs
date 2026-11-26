/*  Class Name      : LedgerGroupListItemDto.cs
 *  Purpose         : Ledger Group tree / grid row
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class LedgerGroupListItemDto
    {
        public int GroupId { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string LedgerGroup { get; set; } = string.Empty;

        public int ParentGroupId { get; set; }

        public string ParentGroup { get; set; } = string.Empty;

        public int NatureId { get; set; }

        public int MainGroupId { get; set; }

        public int ImageId { get; set; }

        public int AccessFlag { get; set; }

        public int SortOrder { get; set; }
    }
}
