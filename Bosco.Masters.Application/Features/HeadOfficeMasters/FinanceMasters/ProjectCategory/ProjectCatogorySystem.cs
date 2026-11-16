using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory;

namespace Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public class ProjectCatogorySystem
    {
        private readonly IProjectCategoryRepository _categories;

        public ProjectCatogorySystem(IProjectCategoryRepository categories)
        {
            _categories = categories;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _categories.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<ProjectCategoryListItemDto>> FetchProjectCatogoryDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _categories.FetchProjectCatogoryDetails(office, cancellationToken);
        }

        public Task<MasterProjectCatogory?> FillProjectCatogoryDetails(HeadOfficeConnection office, int projectCatogoryId, CancellationToken cancellationToken)
        {
            if (projectCatogoryId <= 0)
            {
                return Task.FromResult<MasterProjectCatogory?>(null);
            }

            return _categories.FillProjectCatogoryDetails(office, projectCatogoryId, cancellationToken);
        }

        public Task<IReadOnlyList<ProjectCategoryOptionDto>> FetchCategoryGroups(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _categories.FetchCategoryGroups(office, cancellationToken);
        }

        public Task<IReadOnlyList<ProjectCategoryOptionDto>> FetchITRGroupCategory(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _categories.FetchITRGroupCategory(office, cancellationToken);
        }

        public Task<ProjectCategoryActionResult> SaveProjectCatogoryDetails(HeadOfficeConnection office, ProjectCategorySaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new ProjectCategoryActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.ProjectCatogory.PROJECT_CATOGORY_DETAILS_REQUIRED)
                });
            }

            return _categories.SaveProjectCatogoryDetails(office, request, cancellationToken);
        }

        public Task<ProjectCategoryActionResult> DeleteProjectCatogoryDetails(HeadOfficeConnection office, int projectCatogoryId, CancellationToken cancellationToken)
        {
            return _categories.DeleteProjectCatogoryDetails(office, projectCatogoryId, cancellationToken);
        }
    }
}
