using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project;

namespace Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.Project
{
    public class ProjectSystem
    {
        private readonly IProjectRepository _projects;

        public ProjectSystem(IProjectRepository projects)
        {
            _projects = projects;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _projects.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<ProjectListItemDto>> FetchProjectDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _projects.FetchProjectDetails(office, cancellationToken);
        }

        public Task<MasterProject?> FillProjectDetails(HeadOfficeConnection office, int projectId, CancellationToken cancellationToken)
        {
            if (projectId <= 0)
            {
                return Task.FromResult<MasterProject?>(null);
            }

            return _projects.FillProjectDetails(office, projectId, cancellationToken);
        }

        public Task<IReadOnlyList<ProjectOptionDto>> FetchDivision(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _projects.FetchDivision(office, cancellationToken);
        }

        public Task<IReadOnlyList<ProjectOptionDto>> FetchProjectCategories(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _projects.FetchProjectCategories(office, cancellationToken);
        }

        public Task<IReadOnlyList<ProjectOptionDto>> FetchSocietyNames(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _projects.FetchSocietyNames(office, cancellationToken);
        }

        public Task<ProjectActionResult> SaveProjectDetails(HeadOfficeConnection office, ProjectSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new ProjectActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.Project.PROJECT_DETAILS_REQUIRED)
                });
            }

            return _projects.SaveProjectDetails(office, request, cancellationToken);
        }

        public Task<ProjectActionResult> DeleteProjectDetails(HeadOfficeConnection office, int projectId, CancellationToken cancellationToken)
        {
            return _projects.DeleteProjectDetails(office, projectId, cancellationToken);
        }
    }
}
