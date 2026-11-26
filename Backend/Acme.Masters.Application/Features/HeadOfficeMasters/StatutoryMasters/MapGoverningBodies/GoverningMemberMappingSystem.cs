/*  Class Name      : GoverningMemberMappingSystem.cs
 *  Purpose         : Map Governing Bodies business entry point (legacy ExecutiveMemberSystem.MapGoverningMembers)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies
{
    public class GoverningMemberMappingSystem
    {
        private readonly IMapGoverningBodiesRepository _mappings;

        public GoverningMemberMappingSystem(IMapGoverningBodiesRepository mappings)
        {
            _mappings = mappings;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _mappings.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<LegalEntityOptionDto>> LoadLegalEntityLookup(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _mappings.LoadLegalEntityLookup(office, cancellationToken);
        }

        public Task<IReadOnlyList<GoverningBodyMappingRowDto>?> FetchGoverningMembersByLegalEntity(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken)
        {
            if (customerId <= 0)
            {
                return Task.FromResult<IReadOnlyList<GoverningBodyMappingRowDto>?>(null);
            }

            return _mappings.FetchGoverningMembersByLegalEntity(office, customerId, cancellationToken);
        }

        public Task<GoverningBodyMappingActionResult> MapGoverningMembers(HeadOfficeConnection office, GoverningBodyMappingSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null || request.CustomerId <= 0)
            {
                return Task.FromResult(new GoverningBodyMappingActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(GoverningLegalMessages.MAP_GOVERNING_LEGAL_ENTITY_EMPTY)
                });
            }

            return _mappings.MapGoverningMembers(office, request, cancellationToken);
        }

        public Task<GoverningBodyMappingActionResult> DeleteMappedGoverningMember(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken)
        {
            if (customerId <= 0)
            {
                return Task.FromResult(new GoverningBodyMappingActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(GoverningLegalMessages.MAP_GOVERNING_LEGAL_ENTITY_EMPTY)
                });
            }

            return _mappings.DeleteMappedGoverningMember(office, customerId, cancellationToken);
        }
    }
}
