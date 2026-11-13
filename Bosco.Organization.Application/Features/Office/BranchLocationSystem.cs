/*  Class Name      : BranchLocationSystem.cs
 *  Purpose         : Branch Location use cases (list, get, branches, save, delete)
 *  Author          : Dinesh C
 *  Created on      : 13-Nov-2026
 */
using Bosco.Common;
using Bosco.Organization.Application.Abstractions.Repositories;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Features.Office
{
    public class BranchLocationSystem
    {
        private readonly IBranchLocationRepository _locations;
        private readonly IHeadOfficeRepository _headOffices;

        public BranchLocationSystem(IBranchLocationRepository locations, IHeadOfficeRepository headOffices)
        {
            _locations = locations;
            _headOffices = headOffices;
        }

        public Task<HeadOffice?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _headOffices.HeadOfficeDetailByCode(headOfficeCode, cancellationToken);
        }
        public Task<IReadOnlyList<BranchLocationListItemDto>> FetchBranchLocationAll(HeadOffice office, CancellationToken cancellationToken)
        {
            return _locations.FetchBranchLocationAll(office, cancellationToken);
        }

        public Task<BranchLocationListItemDto?> FillBranchLocationDetails(HeadOffice office, int locationId, CancellationToken cancellationToken)
        {
            if (locationId <= 0)
            {
                return Task.FromResult<BranchLocationListItemDto?>(null);
            }
            return _locations.FillBranchLocationDetails(office, locationId, cancellationToken);
        }
        public Task<IReadOnlyList<BranchOptionDto>> FetchBranch(HeadOffice office, CancellationToken cancellationToken)
        {
            return _locations.FetchBranch(office, cancellationToken);
        }
        public Task<BranchLocationActionResult> SaveBranchLocationDetails(HeadOffice office, BranchLocationSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new BranchLocationActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Office.BranchLocationDetailsRequired)
                });
            }
            return _locations.SaveBranchLocationDetails(office, request, cancellationToken);
        }
        public Task<BranchLocationActionResult> DeleteLocation(HeadOffice office, int locationId, CancellationToken cancellationToken)
        {
            return _locations.DeleteLocation(office, locationId, cancellationToken);
        }



    }
}
