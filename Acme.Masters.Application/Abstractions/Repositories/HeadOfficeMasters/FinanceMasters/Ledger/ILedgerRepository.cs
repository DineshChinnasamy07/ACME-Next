/*  Class Name      : ILedgerRepository.cs
 *  Purpose         : HO tenant MASTER_LEDGER access for LedgerSystem
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Masters.Application.Dtos.HeadOfficeMasters.FinanceMasters.Ledger;
using Acme.Masters.Domain.Entities;
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;

namespace Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Ledger
{
    public interface ILedgerRepository
    {
        Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken);

        Task<IReadOnlyList<LedgerListItemDto>> FetchLedgerDetails(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<MasterLedger?> FillLedgerDetails(HeadOfficeConnection office, int ledgerId, CancellationToken cancellationToken);

        Task<IReadOnlyList<LedgerGroupLookupDto>> LoadLedgerGroupforLedgerLookup(HeadOfficeConnection office, CancellationToken cancellationToken);

        Task<LedgerActionResult> SaveLedgerDetails(HeadOfficeConnection office, LedgerSaveRequest request, CancellationToken cancellationToken);

        Task<LedgerActionResult> DeleteLedgerDetails(HeadOfficeConnection office, int ledgerId, CancellationToken cancellationToken);
    }
}
