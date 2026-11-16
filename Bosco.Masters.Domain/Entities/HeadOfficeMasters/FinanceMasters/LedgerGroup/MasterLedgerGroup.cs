/*  Class Name      : MasterLedgerGroup.cs
 *  Purpose         : HO tenant MASTER_LEDGER_GROUP row for Ledger Group CRUD
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
namespace Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class MasterLedgerGroup
    {
        public int GroupId { get; set; }

        public string? GroupCode { get; set; }

        public string LedgerGroup { get; set; } = string.Empty;

        public int ParentGroupId { get; set; }

        public int NatureId { get; set; }

        public int MainGroupId { get; set; }

        public int ImageId { get; set; }

        public int AccessFlag { get; set; }

        public int SortOrder { get; set; }
    }
}
