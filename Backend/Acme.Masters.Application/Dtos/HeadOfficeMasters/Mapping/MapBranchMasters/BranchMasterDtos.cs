/*  Class Name      : BranchMasterDtos.cs
 *  Purpose         : DTOs for the generic Head Office -> Branch master mapping engine and the branch read.
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
using System.Collections.Generic;

namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapBranchMasters
{
    // A branch in the "map to branch" picker.
    public class BranchMasterBranchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // One master record (ledger, legal entity, …) shown on the HO mapping screen and in the branch list.
    public class BranchMasterRecordDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // Save request: replace the set of master records mapped to one branch for one master type.
    public class BranchMasterMappingSaveRequest
    {
        public int BranchId { get; set; }
        public string MasterType { get; set; } = string.Empty;
        public List<int> MasterIds { get; set; } = new List<int>();
    }

    public class BranchMasterMappingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
