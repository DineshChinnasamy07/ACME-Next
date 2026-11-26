/*  Class Name      : DutyTaxSystem.cs
 *  Purpose         : Business entry point for the Head Office TDS Duty / Tax type master
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.DutyTax
{
    public class DutyTaxSystem
    {
        private readonly IDutyTaxRepository _dutyTaxes;

        public DutyTaxSystem(IDutyTaxRepository dutyTaxes)
        {
            _dutyTaxes = dutyTaxes;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _dutyTaxes.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<DutyTaxListItemDto>> FetchDutyTaxDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _dutyTaxes.FetchDutyTaxDetails(office, cancellationToken);
        }

        public Task<DutyTaxListItemDto?> FillDutyTaxDetails(HeadOfficeConnection office, int tdsDutyTaxTypeId, CancellationToken cancellationToken)
        {
            if (tdsDutyTaxTypeId <= 0)
            {
                return Task.FromResult<DutyTaxListItemDto?>(null);
            }

            return _dutyTaxes.FillDutyTaxDetails(office, tdsDutyTaxTypeId, cancellationToken);
        }

        public Task<DutyTaxActionResult> SaveDutyTaxDetails(HeadOfficeConnection office, DutyTaxSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new DutyTaxActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(TdsMasterMessages.TDS_DETAILS_REQUIRED)
                });
            }

            return _dutyTaxes.SaveDutyTaxDetails(office, request, cancellationToken);
        }

        public Task<DutyTaxActionResult> DeleteDutyTaxDetails(HeadOfficeConnection office, int tdsDutyTaxTypeId, CancellationToken cancellationToken)
        {
            return _dutyTaxes.DeleteDutyTaxDetails(office, tdsDutyTaxTypeId, cancellationToken);
        }
    }
}
