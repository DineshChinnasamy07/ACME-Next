/*  Class Name      : TdsSectionSystem.cs
 *  Purpose         : Business entry point for the Head Office TDS Section master
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.TdsSection
{
    public class TdsSectionSystem
    {
        private readonly ITdsSectionRepository _sections;

        public TdsSectionSystem(ITdsSectionRepository sections)
        {
            _sections = sections;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _sections.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<TdsSectionListItemDto>> FetchTdsSectionDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _sections.FetchTdsSectionDetails(office, cancellationToken);
        }

        public Task<TdsSectionListItemDto?> FillTdsSectionDetails(HeadOfficeConnection office, int tdsSectionId, CancellationToken cancellationToken)
        {
            if (tdsSectionId <= 0)
            {
                return Task.FromResult<TdsSectionListItemDto?>(null);
            }

            return _sections.FillTdsSectionDetails(office, tdsSectionId, cancellationToken);
        }

        public Task<TdsSectionActionResult> SaveTdsSectionDetails(HeadOfficeConnection office, TdsSectionSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new TdsSectionActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(TdsMasterMessages.TDS_DETAILS_REQUIRED)
                });
            }

            return _sections.SaveTdsSectionDetails(office, request, cancellationToken);
        }

        public Task<TdsSectionActionResult> DeleteTdsSectionDetails(HeadOfficeConnection office, int tdsSectionId, CancellationToken cancellationToken)
        {
            return _sections.DeleteTdsSectionDetails(office, tdsSectionId, cancellationToken);
        }
    }
}
