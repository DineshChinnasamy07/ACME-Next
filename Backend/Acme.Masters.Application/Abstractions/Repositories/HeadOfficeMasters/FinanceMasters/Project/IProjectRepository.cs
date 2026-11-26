using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Project;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Project
{
    public interface IProjectRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<ProjectListItemDto>> FetchProjectDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<MasterProject?> FillProjectDetails(HeadOfficeConnection office, int projectId, CancellationToken cancellationToken);

        Task<IReadOnlyList<ProjectOptionDto>> FetchDivision(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<IReadOnlyList<ProjectOptionDto>> FetchProjectCategories(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<IReadOnlyList<ProjectOptionDto>> FetchSocietyNames(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<ProjectActionResult> SaveProjectDetails(HeadOfficeConnection office, ProjectSaveRequest request, CancellationToken cancellationToken);

        Task<ProjectActionResult> DeleteProjectDetails(HeadOfficeConnection office, int projectId, CancellationToken cancellationToken);
    }
}
