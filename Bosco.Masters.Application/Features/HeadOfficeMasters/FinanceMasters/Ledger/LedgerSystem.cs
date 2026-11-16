/*  Class Name      : LedgerSystem.cs
 *  Purpose         : Ledger use cases (list, get, group lookup, save, delete)
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Common;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Ledger;
using Bosco.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Ledger;
using Bosco.Masters.Domain.Entities;
using Bosco.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;

namespace Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.Ledger
{
    public class LedgerSystem
    {
        private readonly ILedgerRepository _ledgers;

        public LedgerSystem(ILedgerRepository ledgers)
        {
            _ledgers = ledgers;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _ledgers.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<LedgerListItemDto>> FetchLedgerDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _ledgers.FetchLedgerDetails(office, cancellationToken);
        }

        public Task<MasterLedger?> FillLedgerDetails(HeadOfficeConnection office, int ledgerId, CancellationToken cancellationToken)
        {
            if (ledgerId <= 0)
            {
                return Task.FromResult<MasterLedger?>(null);
            }

            return _ledgers.FillLedgerDetails(office, ledgerId, cancellationToken);
        }

        public Task<IReadOnlyList<LedgerGroupLookupDto>> LoadLedgerGroupforLedgerLookup(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _ledgers.LoadLedgerGroupforLedgerLookup(office, cancellationToken);
        }

        public Task<LedgerActionResult> SaveLedgerDetails(HeadOfficeConnection office, LedgerSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new LedgerActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.Ledger.LEDGER_DETAILS_REQUIRED)
                });
            }

            return _ledgers.SaveLedgerDetails(office, request, cancellationToken);
        }

        public Task<LedgerActionResult> DeleteLedgerDetails(HeadOfficeConnection office, int ledgerId, CancellationToken cancellationToken)
        {
            return _ledgers.DeleteLedgerDetails(office, ledgerId, cancellationToken);
        }
    }
}
