/*  Class Name      : BranchOfficeSystem.cs
 *  Purpose         : Portal Branch Office use cases (list, get, save)
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using Bosco.Organization.Application.Abstractions.Repositories;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Features.Office
{
    public class BranchOfficeSystem
    {
        private readonly IBranchOfficeRepository _branchOffices;

        public BranchOfficeSystem(IBranchOfficeRepository branchOffices)
        {
            _branchOffices = branchOffices;
        }

        public Task<IReadOnlyList<BranchOfficeListItemDto>> FetchBranchOfficeDetails(CancellationToken cancellationToken)
        {
            return _branchOffices.FetchBranchOfficeDetails(cancellationToken);
        }

        public Task<BranchOffice?> FillBranchOfficeDetails(int branchOfficeId, CancellationToken cancellationToken)
        {
            if (branchOfficeId <= 0)
            {
                return Task.FromResult<BranchOffice?>(null);
            }

            return _branchOffices.FillBranchOfficeDetails(branchOfficeId, cancellationToken);
        }

        public Task<BranchOfficeActionResult> SaveBranchOfficeDetails(BranchOfficeSaveRequest? request, int modifiedBy, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new BranchOfficeActionResult
                {
                    Success = false,
                    Message = "Branch Office details are required."
                });
            }

            return _branchOffices.SaveBranchOfficeDetails(request, modifiedBy, cancellationToken);
        }
    }
}
