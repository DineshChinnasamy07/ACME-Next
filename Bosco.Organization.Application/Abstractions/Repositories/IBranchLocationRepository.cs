
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Abstractions.Repositories
{
    public interface IBranchLocationRepository
    {
        Task<IReadOnlyList<BranchLocationListItemDto>> FetchBranchLocationAll(HeadOffice office, CancellationToken cancellationToken);
        Task<BranchLocationListItemDto?> FillBranchLocationDetails(HeadOffice office,int locationId, CancellationToken cancellationToken);

        Task<IReadOnlyList<BranchOptionDto>> FetchBranch(HeadOffice office,CancellationToken cancellationToken);

        Task<BranchLocationActionResult> SaveBranchLocationDetails(HeadOffice office, BranchLocationSaveRequest  request, CancellationToken cancellationToken);

        Task<BranchLocationActionResult> DeleteLocation(HeadOffice office, int locationId, CancellationToken cancellationToken);
    }
}
