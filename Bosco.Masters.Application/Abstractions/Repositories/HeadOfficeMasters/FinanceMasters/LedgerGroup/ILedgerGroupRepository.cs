/*  Class Name      : ILedgerGroupRepository.cs
 *  Purpose         : HO tenant MASTER_LEDGER_GROUP access for LedgerGroupSystem
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;

namespace Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public interface ILedgerGroupRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<LedgerGroupListItemDto>> GetLedgerGroupSource(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<IReadOnlyList<LedgerGroupOptionDto>> LoadLedgerGroupSource(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<MasterLedgerGroup?> FillLedgerGroupDetails(HeadOfficeConnection office, int groupId, CancellationToken cancellationToken);

        Task<IReadOnlyList<LedgerUnderGroupDto>> GetLedgerList(HeadOfficeConnection office, string groupIds, CancellationToken cancellationToken);

        Task<LedgerGroupActionResult> SaveLedgerGroupDetails(HeadOfficeConnection office, LedgerGroupSaveRequest request, CancellationToken cancellationToken);

        Task<LedgerGroupActionResult> DeleteLedgerGroup(HeadOfficeConnection office, int groupId, CancellationToken cancellationToken);
    }
}
