/*  Class Name      : IMapProjectsRepository.cs
 *  Purpose         : Project Mapping data access on the HO tenant database
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapProjects;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.Mapping.MapProjects
{
    public interface IMapProjectsRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<ProjectMappingLookupsDto> FetchMappingLookups(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<ProjectMappingDetailDto?> FillProjectMapping(HeadOfficeConnection office, int projectId, CancellationToken cancellationToken);

        Task<ProjectMappingActionResult> SaveProjectMapping(HeadOfficeConnection office, ProjectMappingSaveRequest request, CancellationToken cancellationToken);
    }
}
