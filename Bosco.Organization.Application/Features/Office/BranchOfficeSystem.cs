/*  Class Name      : BranchOfficeSystem.cs
 *  Purpose         : Portal Branch Office use cases (list, get, save, status, delete)
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using Bosco.Common;
using Bosco.Organization.Application.Abstractions.Repositories;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Features.Office
{
    public class BranchOfficeSystem
    {
        private readonly IBranchOfficeRepository _branchOffices;

        public BranchOfficeSystem(IBranchOfficeRepository branchOffices)
        {
            _branchOffices = branchOffices;
        }

        public Task<IReadOnlyList<BranchOfficeListItemDto>> FetchBranchOfficeDetails(CancellationToken cancellationToken)
        {
            return _branchOffices.FetchBranchOfficeDetails(cancellationToken);
        }

        public Task<BranchOffice?> FillBranchOfficeDetails(int branchOfficeId, CancellationToken cancellationToken)
        {
            if (branchOfficeId <= 0)
            {
                return Task.FromResult<BranchOffice?>(null);
            }

            return _branchOffices.FillBranchOfficeDetails(branchOfficeId, cancellationToken);
        }

        public Task<BranchOfficeActionResult> SaveBranchOfficeDetails(BranchOfficeSaveRequest? request, int modifiedBy, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new BranchOfficeActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDetailsRequired)
                });
            }

            return _branchOffices.SaveBranchOfficeDetails(request, modifiedBy, cancellationToken);
        }

        public Task<BranchOfficeActionResult> UpdateOfficeStatus(string branchOfficeCode,int status, CancellationToken cancellationToken) 
        {
            return _branchOffices.UpdateOfficeStatus(branchOfficeCode, status, cancellationToken);
        }

        public Task<BranchOfficeActionResult> DeleteBranchOfficeDetails(string branchOfficeCode, CancellationToken cancellationToken)
        {
            return _branchOffices.DeleteBranchOfficeDetails(branchOfficeCode, cancellationToken);
        }


    }
}
