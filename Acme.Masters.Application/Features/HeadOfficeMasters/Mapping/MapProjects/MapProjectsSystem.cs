/*  Class Name      : MapProjectsSystem.cs
 *  Purpose         : Project Mapping business entry point (project → branches with location, society, category, division, period, user in charge)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Common;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.Mapping.MapProjects;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.Mapping.MapProjects
{
    public class MapProjectsSystem
    {
        private readonly IMapProjectsRepository _mappings;

        public MapProjectsSystem(IMapProjectsRepository mappings)
        {
            _mappings = mappings;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _mappings.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<ProjectMappingLookupsDto> FetchMappingLookups(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _mappings.FetchMappingLookups(office, cancellationToken);
        }

        public Task<ProjectMappingDetailDto?> FillProjectMapping(HeadOfficeConnection office, int projectId, CancellationToken cancellationToken)
        {
            if (projectId <= 0)
            {
                return Task.FromResult<ProjectMappingDetailDto?>(null);
            }

            return _mappings.FillProjectMapping(office, projectId, cancellationToken);
        }

        public Task<ProjectMappingActionResult> SaveProjectMapping(HeadOfficeConnection office, ProjectMappingSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null || request.ProjectId <= 0)
            {
                return Task.FromResult(new ProjectMappingActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.ProjectMapping.PROJECT_MAPPING_PROJECT_EMPTY),
                    RowIndex = -1
                });
            }

            return _mappings.SaveProjectMapping(office, request, cancellationToken);
        }
    }
}
