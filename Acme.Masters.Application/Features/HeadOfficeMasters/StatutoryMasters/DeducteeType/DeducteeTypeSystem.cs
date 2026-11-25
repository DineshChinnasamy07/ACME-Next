/*  Class Name      : DeducteeTypeSystem.cs
 *  Purpose         : Business entry point for the Head Office TDS Deductee Type master
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.DeducteeType
{
    public class DeducteeTypeSystem
    {
        private readonly IDeducteeTypeRepository _deducteeTypes;

        public DeducteeTypeSystem(IDeducteeTypeRepository deducteeTypes)
        {
            _deducteeTypes = deducteeTypes;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _deducteeTypes.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<DeducteeTypeListItemDto>> FetchDeducteeTypeDetails(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _deducteeTypes.FetchDeducteeTypeDetails(office, cancellationToken);
        }

        public Task<DeducteeTypeListItemDto?> FillDeducteeTypeDetails(HeadOfficeConnection office, int deducteeTypeId, CancellationToken cancellationToken)
        {
            if (deducteeTypeId <= 0)
            {
                return Task.FromResult<DeducteeTypeListItemDto?>(null);
            }

            return _deducteeTypes.FillDeducteeTypeDetails(office, deducteeTypeId, cancellationToken);
        }

        public Task<DeducteeTypeActionResult> SaveDeducteeTypeDetails(HeadOfficeConnection office, DeducteeTypeSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new DeducteeTypeActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(TdsMasterMessages.TDS_DETAILS_REQUIRED)
                });
            }

            return _deducteeTypes.SaveDeducteeTypeDetails(office, request, cancellationToken);
        }

        public Task<DeducteeTypeActionResult> DeleteDeducteeTypeDetails(HeadOfficeConnection office, int deducteeTypeId, CancellationToken cancellationToken)
        {
            return _deducteeTypes.DeleteDeducteeTypeDetails(office, deducteeTypeId, cancellationToken);
        }
    }
}
