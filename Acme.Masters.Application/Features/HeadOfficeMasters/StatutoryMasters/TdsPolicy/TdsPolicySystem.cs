/*  Class Name      : TdsPolicySystem.cs
 *  Purpose         : Head Office Statutory Masters - TDS Policy business entry points
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsPolicySystem
    {
        private readonly ITdsPolicyRepository _policies;

        public TdsPolicySystem(ITdsPolicyRepository policies)
        {
            _policies = policies;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _policies.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<TdsPolicyListItemDto>> FetchTdsPolicyDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _policies.FetchTdsPolicyDetails(office, cancellationToken);
        }

        public Task<TdsPolicyLookupsDto> FetchTdsPolicyLookups(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _policies.FetchTdsPolicyLookups(office, cancellationToken);
        }

        public Task<TdsPolicyDetailDto?> FillTdsPolicyDetails(HeadOfficeConnection office, int tdsPolicyId, CancellationToken cancellationToken)
        {
            if (tdsPolicyId <= 0)
            {
                return Task.FromResult<TdsPolicyDetailDto?>(null);
            }

            return _policies.FillTdsPolicyDetails(office, tdsPolicyId, cancellationToken);
        }

        public Task<TdsPolicyActionResult> SaveTdsPolicyDetails(HeadOfficeConnection office, TdsPolicySaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new TdsPolicyActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(TdsPolicyMessages.TDS_POLICY_DETAILS_REQUIRED)
                });
            }

            return _policies.SaveTdsPolicyDetails(office, request, cancellationToken);
        }

        public Task<TdsPolicyActionResult> DeleteTdsPolicyDetails(HeadOfficeConnection office, int tdsPolicyId, CancellationToken cancellationToken)
        {
            return _policies.DeleteTdsPolicyDetails(office, tdsPolicyId, cancellationToken);
        }
    }
}
