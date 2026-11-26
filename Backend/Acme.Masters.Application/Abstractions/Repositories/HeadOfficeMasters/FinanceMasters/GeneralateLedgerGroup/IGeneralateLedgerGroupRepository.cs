using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public interface IGeneralateLedgerGroupRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<GeneralateLedgerListItemDto>> FetchAllGeneralateLedgers(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<IReadOnlyList<GeneralateLedgerOptionDto>> FetchAllGeneralateParentLedgers(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<CongregationLedger?> FetchGeneralateLedgerById(HeadOfficeConnection office, int conLedgerId, CancellationToken cancellationToken);

        Task<GeneralateLedgerActionResult> SaveGeneralateLedger(HeadOfficeConnection office, GeneralateLedgerSaveRequest request, CancellationToken cancellationToken);

        Task<GeneralateLedgerActionResult> DeleteLedger(HeadOfficeConnection office, int conLedgerId, CancellationToken cancellationToken);
    }
}
