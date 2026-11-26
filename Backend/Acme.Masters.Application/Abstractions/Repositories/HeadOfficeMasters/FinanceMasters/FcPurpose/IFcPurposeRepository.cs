using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.FcPurpose;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.FcPurpose
{
    public interface IFcPurposeRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<FcPurposeListItemDto>> FetchPurposeDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<MasterContributionHead?> FillPurposeDetails(HeadOfficeConnection office, int contributionId, CancellationToken cancellationToken);

        Task<FcPurposeActionResult> SavePurposeDetails(HeadOfficeConnection office, FcPurposeSaveRequest request, CancellationToken cancellationToken);

        Task<FcPurposeActionResult> DeletePurposeDetails(HeadOfficeConnection office, int contributionId, CancellationToken cancellationToken);
    }
}
