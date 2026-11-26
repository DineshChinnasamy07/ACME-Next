using Acme.Common;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.FcPurpose;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.FcPurpose
{
    public class PurposeSystem
    {
        private readonly IFcPurposeRepository _purposes;

        public PurposeSystem(IFcPurposeRepository purposes)
        {
            _purposes = purposes;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _purposes.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<FcPurposeListItemDto>> FetchPurposeDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _purposes.FetchPurposeDetails(office, cancellationToken);
        }

        public Task<MasterContributionHead?> FillPurposeDetails(HeadOfficeConnection office, int contributionId, CancellationToken cancellationToken)
        {
            if (contributionId <= 0)
            {
                return Task.FromResult<MasterContributionHead?>(null);
            }

            return _purposes.FillPurposeDetails(office, contributionId, cancellationToken);
        }

        public Task<FcPurposeActionResult> SavePurposeDetails(HeadOfficeConnection office, FcPurposeSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new FcPurposeActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.Purposes.PURPOSE_DETAILS_REQUIRED)
                });
            }

            return _purposes.SavePurposeDetails(office, request, cancellationToken);
        }

        public Task<FcPurposeActionResult> DeletePurposeDetails(HeadOfficeConnection office, int contributionId, CancellationToken cancellationToken)
        {
            return _purposes.DeletePurposeDetails(office, contributionId, cancellationToken);
        }
    }
}
