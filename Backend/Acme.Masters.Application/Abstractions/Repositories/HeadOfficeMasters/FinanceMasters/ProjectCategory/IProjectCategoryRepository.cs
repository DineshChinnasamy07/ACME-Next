using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.ProjectCategory;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.ProjectCategory
{
    public interface IProjectCategoryRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<ProjectCategoryListItemDto>> FetchProjectCatogoryDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<MasterProjectCatogory?> FillProjectCatogoryDetails(HeadOfficeConnection office, int projectCatogoryId, CancellationToken cancellationToken);

        Task<IReadOnlyList<ProjectCategoryOptionDto>> FetchCategoryGroups(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<IReadOnlyList<ProjectCategoryOptionDto>> FetchITRGroupCategory(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<ProjectCategoryActionResult> SaveProjectCatogoryDetails(HeadOfficeConnection office, ProjectCategorySaveRequest request, CancellationToken cancellationToken);

        Task<ProjectCategoryActionResult> DeleteProjectCatogoryDetails(HeadOfficeConnection office, int projectCatogoryId, CancellationToken cancellationToken);
    }
}
