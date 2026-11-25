/*  Class Name      : NatureOfPaymentSystem.cs
 *  Purpose         : Business entry point for the Head Office TDS Nature of Payments master
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public class NatureOfPaymentSystem
    {
        private readonly INatureOfPaymentRepository _natures;

        public NatureOfPaymentSystem(INatureOfPaymentRepository natures)
        {
            _natures = natures;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _natures.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<NatureOfPaymentListItemDto>> FetchNatureOfPaymentDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _natures.FetchNatureOfPaymentDetails(office, cancellationToken);
        }

        public Task<IReadOnlyList<NatureOfPaymentSectionOptionDto>> FetchSectionCodes(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _natures.FetchSectionCodes(office, cancellationToken);
        }

        public Task<NatureOfPaymentListItemDto?> FillNatureOfPaymentDetails(HeadOfficeConnection office, int naturePayId, CancellationToken cancellationToken)
        {
            if (naturePayId <= 0)
            {
                return Task.FromResult<NatureOfPaymentListItemDto?>(null);
            }

            return _natures.FillNatureOfPaymentDetails(office, naturePayId, cancellationToken);
        }

        public Task<NatureOfPaymentActionResult> SaveNatureOfPaymentDetails(HeadOfficeConnection office, NatureOfPaymentSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new NatureOfPaymentActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(TdsMasterMessages.TDS_DETAILS_REQUIRED)
                });
            }

            return _natures.SaveNatureOfPaymentDetails(office, request, cancellationToken);
        }

        public Task<NatureOfPaymentActionResult> DeleteNatureOfPaymentDetails(HeadOfficeConnection office, int naturePayId, CancellationToken cancellationToken)
        {
            return _natures.DeleteNatureOfPaymentDetails(office, naturePayId, cancellationToken);
        }
    }
}
