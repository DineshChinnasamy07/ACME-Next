/*  Class Name      : IBranchMasterMappingRepository.cs
 *  Purpose         : Generic Head Office -> Branch master mapping (write) and branch read.
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapBranchMasters;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.Mapping.MapBranchMasters
{
    public interface IBranchMasterMappingRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        // HO mapping screen.
        Task<IReadOnlyList<BranchMasterBranchDto>> FetchBranches(HeadOfficeConnection office, CancellationToken cancellationToken);
        Task<IReadOnlyList<BranchMasterRecordDto>> FetchRecords(HeadOfficeConnection office, string masterType, CancellationToken cancellationToken);
        Task<IReadOnlyList<int>> FetchMappedIds(HeadOfficeConnection office, string masterType, int branchId, CancellationToken cancellationToken);
        Task<BranchMasterMappingResult> SaveMapping(HeadOfficeConnection office, string masterType, int branchId, IReadOnlyList<int> masterIds, CancellationToken cancellationToken);

        // Branch login read: the master records mapped to that branch.
        Task<IReadOnlyList<BranchMasterRecordDto>> FetchBranchMasters(HeadOfficeConnection office, string branchCode, string masterType, CancellationToken cancellationToken);
    }
}
