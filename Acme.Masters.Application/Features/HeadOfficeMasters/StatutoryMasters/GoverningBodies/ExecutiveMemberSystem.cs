/*  Class Name      : ExecutiveMemberSystem.cs
 *  Purpose         : Governing Bodies business entry point (legacy Bosco.Model ExecutiveMemberSystem)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.GoverningBodies
{
    public class ExecutiveMemberSystem
    {
        private readonly IGoverningBodiesRepository _members;

        public ExecutiveMemberSystem(IGoverningBodiesRepository members)
        {
            _members = members;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _members.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<GoverningBodyListItemDto>> FetchExecutiveMemberDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _members.FetchExecutiveMemberDetails(office, cancellationToken);
        }

        public Task<GoverningBodyDetailDto?> FillExecutiveMemberDetails(HeadOfficeConnection office, int executiveId, CancellationToken cancellationToken)
        {
            if (executiveId <= 0)
            {
                return Task.FromResult<GoverningBodyDetailDto?>(null);
            }

            return _members.FillExecutiveMemberDetails(office, executiveId, cancellationToken);
        }

        public Task<GoverningBodyActionResult> SaveExecutiveMemberDetails(HeadOfficeConnection office, GoverningBodySaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new GoverningBodyActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(GoverningLegalMessages.GOVERNING_BODY_DETAILS_REQUIRED)
                });
            }

            return _members.SaveExecutiveMemberDetails(office, request, cancellationToken);
        }

        public Task<GoverningBodyActionResult> DeleteExecutiveMemberDetails(HeadOfficeConnection office, int executiveId, CancellationToken cancellationToken)
        {
            return _members.DeleteExecutiveMemberDetails(office, executiveId, cancellationToken);
        }
    }
}
