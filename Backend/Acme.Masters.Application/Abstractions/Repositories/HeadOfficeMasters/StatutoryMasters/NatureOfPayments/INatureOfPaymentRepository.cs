/*  Class Name      : INatureOfPaymentRepository.cs
 *  Purpose         : Data access contract for the Head Office TDS Nature of Payments master
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.NatureOfPayments
{
    public interface INatureOfPaymentRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<NatureOfPaymentListItemDto>> FetchNatureOfPaymentDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<IReadOnlyList<NatureOfPaymentSectionOptionDto>> FetchSectionCodes(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<NatureOfPaymentListItemDto?> FillNatureOfPaymentDetails(HeadOfficeConnection office, int naturePayId, CancellationToken cancellationToken);

        Task<NatureOfPaymentActionResult> SaveNatureOfPaymentDetails(HeadOfficeConnection office, NatureOfPaymentSaveRequest request, CancellationToken cancellationToken);

        Task<NatureOfPaymentActionResult> DeleteNatureOfPaymentDetails(HeadOfficeConnection office, int naturePayId, CancellationToken cancellationToken);
    }
}
