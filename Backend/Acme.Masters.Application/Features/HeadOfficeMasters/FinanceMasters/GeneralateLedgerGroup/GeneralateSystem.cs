using Acme.Common;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup
{
    public class GeneralateSystem
    {
        private readonly IGeneralateLedgerGroupRepository _ledgers;

        public GeneralateSystem(IGeneralateLedgerGroupRepository ledgers)
        {
            _ledgers = ledgers;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _ledgers.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<GeneralateLedgerListItemDto>> FetchAllGeneralateLedgers(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _ledgers.FetchAllGeneralateLedgers(office, cancellationToken);
        }

        public Task<IReadOnlyList<GeneralateLedgerOptionDto>> FetchAllGeneralateParentLedgers(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _ledgers.FetchAllGeneralateParentLedgers(office, cancellationToken);
        }

        public Task<CongregationLedger?> FetchGeneralateLedgerById(HeadOfficeConnection office, int conLedgerId, CancellationToken cancellationToken)
        {
            if (conLedgerId <= 0)
            {
                return Task.FromResult<CongregationLedger?>(null);
            }

            return _ledgers.FetchGeneralateLedgerById(office, conLedgerId, cancellationToken);
        }

        public Task<GeneralateLedgerActionResult> SaveGeneralateLedger(HeadOfficeConnection office, GeneralateLedgerSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new GeneralateLedgerActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Master.GeneralateLedger.GENERALATE_LEDGER_DETAILS_REQUIRED)
                });
            }

            return _ledgers.SaveGeneralateLedger(office, request, cancellationToken);
        }

        public Task<GeneralateLedgerActionResult> DeleteLedger(HeadOfficeConnection office, int conLedgerId, CancellationToken cancellationToken)
        {
            return _ledgers.DeleteLedger(office, conLedgerId, cancellationToken);
        }
    }
}
