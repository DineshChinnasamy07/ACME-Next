/*  Class Name      : BranchMasterMappingSystem.cs
 *  Purpose         : Validation / orchestration for the generic Head Office -> Branch master mapping.
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.Mapping.MapBranchMasters;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapBranchMasters;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.Mapping.MapBranchMasters
{
    public class BranchMasterMappingSystem
    {
        // Master types this engine supports. Kept here (application layer) so the controller can validate
        // without referencing the infrastructure repository.
        private static readonly HashSet<string> Supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ledger", "ledgergroup", "projectcategory", "fcpurpose", "legalentity"
        };

        private readonly IBranchMasterMappingRepository _repository;

        public BranchMasterMappingSystem(IBranchMasterMappingRepository repository)
        {
            _repository = repository;
        }

        public static bool IsSupportedType(string? masterType)
        {
            return !string.IsNullOrWhiteSpace(masterType) && Supported.Contains(masterType.Trim());
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _repository.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<BranchMasterBranchDto>> FetchBranches(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _repository.FetchBranches(office, cancellationToken);
        }

        public Task<IReadOnlyList<BranchMasterRecordDto>> FetchRecords(HeadOfficeConnection office, string masterType, CancellationToken cancellationToken)
        {
            return _repository.FetchRecords(office, masterType, cancellationToken);
        }

        public Task<IReadOnlyList<int>> FetchMappedIds(HeadOfficeConnection office, string masterType, int branchId, CancellationToken cancellationToken)
        {
            return _repository.FetchMappedIds(office, masterType, branchId, cancellationToken);
        }

        public async Task<BranchMasterMappingResult> SaveMapping(HeadOfficeConnection office, BranchMasterMappingSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new BranchMasterMappingResult { Success = false, Message = "No data was received." };
            }

            if (!IsSupportedType(request.MasterType))
            {
                return new BranchMasterMappingResult { Success = false, Message = "This master type cannot be mapped yet." };
            }

            if (request.BranchId <= 0)
            {
                return new BranchMasterMappingResult { Success = false, Message = "Select a branch first." };
            }

            return await _repository.SaveMapping(office, request.MasterType, request.BranchId, request.MasterIds ?? new List<int>(), cancellationToken);
        }

        public Task<IReadOnlyList<BranchMasterRecordDto>> FetchBranchMasters(HeadOfficeConnection office, string branchCode, string masterType, CancellationToken cancellationToken)
        {
            return _repository.FetchBranchMasters(office, branchCode, masterType, cancellationToken);
        }
    }
}
