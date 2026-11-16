/*  Class Name      : LedgerGroupSystem.cs
 *  Purpose         : Ledger Group use cases (list, lookup, get, ledgers, save, delete)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;

namespace Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class LedgerGroupSystem
    {
        private readonly ILedgerGroupRepository _groups;

        public LedgerGroupSystem(ILedgerGroupRepository groups)
        {
            _groups = groups;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _groups.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<LedgerGroupListItemDto>> GetLedgerGroupSource(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _groups.GetLedgerGroupSource(office, cancellationToken);
        }

        public Task<IReadOnlyList<LedgerGroupOptionDto>> LoadLedgerGroupSource(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _groups.LoadLedgerGroupSource(office, cancellationToken);
        }

        public Task<MasterLedgerGroup?> FillLedgerGroupDetails(HeadOfficeConnection office, int groupId, CancellationToken cancellationToken)
        {
            if (groupId <= 0)
            {
                return Task.FromResult<MasterLedgerGroup?>(null);
            }

            return _groups.FillLedgerGroupDetails(office, groupId, cancellationToken);
        }

        public Task<IReadOnlyList<LedgerUnderGroupDto>> GetLedgerList(HeadOfficeConnection office, string? groupIds, CancellationToken cancellationToken)
        {
            return _groups.GetLedgerList(office, groupIds ?? string.Empty, cancellationToken);
        }

        public Task<LedgerGroupActionResult> SaveLedgerGroupDetails(HeadOfficeConnection office, LedgerGroupSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new LedgerGroupActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.Group.GROUP_DETAILS_REQUIRED)
                });
            }

            return _groups.SaveLedgerGroupDetails(office, request, cancellationToken);
        }

        public Task<LedgerGroupActionResult> DeleteLedgerGroup(HeadOfficeConnection office, int groupId, CancellationToken cancellationToken)
        {
            return _groups.DeleteLedgerGroup(office, groupId, cancellationToken);
        }
    }
}
