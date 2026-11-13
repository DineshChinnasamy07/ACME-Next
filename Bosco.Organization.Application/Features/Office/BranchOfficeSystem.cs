/*  Class Name      : BranchOfficeSystem.cs
 *  Purpose         : Branch Office use cases (list, get, save, status, delete) with dual-write into the HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using Bosco.Common;
using Bosco.Organization.Application.Abstractions.Repositories;
using Bosco.Organization.Application.Abstractions.Services;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Features.Office
{
    public class BranchOfficeSystem
    {
        private readonly IBranchOfficeRepository _branchOffices;
        private readonly IHeadOfficeRepository _headOffices;
        private readonly IBranchOfficeTenantWriter _tenantWriter;

        public BranchOfficeSystem(IBranchOfficeRepository branchOffices, IHeadOfficeRepository headOffices, IBranchOfficeTenantWriter tenantWriter)
        {
            _branchOffices = branchOffices;
            _headOffices = headOffices;
            _tenantWriter = tenantWriter;
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

        public async Task<BranchOfficeActionResult> SaveBranchOfficeDetails(BranchOfficeSaveRequest? request, int modifiedBy, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new BranchOfficeActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDetailsRequired)
                };
            }

            BranchOfficeActionResult portalResult = await _branchOffices.SaveBranchOfficeDetails(request, modifiedBy, cancellationToken);
            if (!portalResult.Success)
            {
                return portalResult;
            }

            BranchOffice? portalBranch = await _branchOffices.FillBranchOfficeDetails(portalResult.BranchOfficeId, cancellationToken);
            if (portalBranch == null)
            {
                return portalResult;
            }

            HeadOffice? headOffice = await _headOffices.HeadOfficeDetailByCode(portalBranch.HeadOfficeCode, cancellationToken);
            if (headOffice == null)
            {
                return portalResult;
            }

            BranchOfficeActionResult tenantResult = await _tenantWriter.SaveBranchOfficeDetails(headOffice, portalBranch, cancellationToken);
            return WithTenantNote(portalResult, tenantResult);
        }

        public async Task<BranchOfficeActionResult> UpdateOfficeStatus(string branchOfficeCode, int status, CancellationToken cancellationToken)
        {
            BranchOfficeActionResult portalResult = await _branchOffices.UpdateOfficeStatus(branchOfficeCode, status, cancellationToken);
            if (!portalResult.Success)
            {
                return portalResult;
            }

            BranchOffice? portalBranch = await _branchOffices.FillBranchOfficeDetails(portalResult.BranchOfficeId, cancellationToken);
            if (portalBranch == null)
            {
                return portalResult;
            }

            HeadOffice? headOffice = await _headOffices.HeadOfficeDetailByCode(portalBranch.HeadOfficeCode, cancellationToken);
            if (headOffice == null)
            {
                return portalResult;
            }

            BranchOfficeActionResult tenantResult = await _tenantWriter.UpdateOfficeStatus(headOffice, portalBranch, status, cancellationToken);
            return WithTenantNote(portalResult, tenantResult);
        }

        public async Task<BranchOfficeActionResult> DeleteBranchOfficeDetails(string branchOfficeCode, CancellationToken cancellationToken)
        {
            // The head office code is needed to reach the tenant DB, so read it before the portal row is gone.
            string code = (branchOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            IReadOnlyList<BranchOfficeListItemDto> rows = await _branchOffices.FetchBranchOfficeDetails(cancellationToken);
            BranchOfficeListItemDto? match = rows.FirstOrDefault(row => (row.HeadOfficeCode + row.Code).Trim().ToLowerInvariant() == code);

            BranchOfficeActionResult portalResult = await _branchOffices.DeleteBranchOfficeDetails(branchOfficeCode ?? string.Empty, cancellationToken);
            if (!portalResult.Success || match == null)
            {
                return portalResult;
            }

            HeadOffice? headOffice = await _headOffices.HeadOfficeDetailByCode(match.HeadOfficeCode, cancellationToken);
            if (headOffice == null)
            {
                return portalResult;
            }

            BranchOfficeActionResult tenantResult = await _tenantWriter.DeleteBranchOfficeDetails(headOffice, code, cancellationToken);
            return WithTenantNote(portalResult, tenantResult);
        }

        // The portal save already worked. A tenant problem is added to the message instead of failing the action.
        private static BranchOfficeActionResult WithTenantNote(BranchOfficeActionResult portalResult, BranchOfficeActionResult tenantResult)
        {
            if (!tenantResult.Success)
            {
                portalResult.Message = (portalResult.Message + " " + tenantResult.Message).Trim();
            }

            return portalResult;
        }
    }
}
