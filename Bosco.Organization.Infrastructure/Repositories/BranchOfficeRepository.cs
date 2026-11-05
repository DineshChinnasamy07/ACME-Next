/*  Class Name      : BranchOfficeRepository.cs
 *  Purpose         : Portal branch_office list, get, and SaveBranchOfficeDetails
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
using Bosco.Common;
using Bosco.Organization.Application.Abstractions.Repositories;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;
using Bosco.Organization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bosco.Organization.Infrastructure.Repositories
{
    public class BranchOfficeRepository : IBranchOfficeRepository
    {
        private readonly OrganizationDbContext _portal;

        #region Properties

        #endregion

        #region methods
        private static string? TrimOrNull(string? value)
        {
            if (value == null)
            {
                return null;
            }

            string trimmed = value.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        private static BranchOfficeActionResult Ok(string message, int branchOfficeId)
        {
            return new BranchOfficeActionResult
            {
                Success = true,
                Message = message,
                BranchOfficeId = branchOfficeId
            };
        }

        private static BranchOfficeActionResult Fail(string message)
        {
            return new BranchOfficeActionResult
            {
                Success = false,
                Message = message
            };
        }

        #endregion

        #region Implementation of IBranchOfficeRepository
        public BranchOfficeRepository(OrganizationDbContext portal)
        {
            _portal = portal;
        }

        public async Task<IReadOnlyList<BranchOfficeListItemDto>> FetchBranchOfficeDetails(CancellationToken cancellationToken)
        {
            return await _portal.BranchOffices
                .AsNoTracking()
                .OrderBy(branch => branch.Status)
                .ThenBy(branch => branch.BranchOfficeCode)
                .Select(branch => new BranchOfficeListItemDto
                {
                    BranchOfficeId = branch.BranchOfficeId,
                    HeadOfficeCode = branch.HeadOfficeCode ?? string.Empty,
                    Code = branch.BranchPartCode ?? string.Empty,
                    Name = branch.BranchOfficeName ?? string.Empty,
                    DeploymentType = branch.DeploymentType ?? 0,
                    SystemType = branch.DeploymentType == 1 ? "Client-Server" : "Standalone",
                    Status = branch.Status,
                    OfficeStatus = branch.Status == 1 ? "Created"
                        : branch.Status == 2 ? "Activated"
                        : "DeActivated",
                    IsSubBranch = branch.IsSubBranch,
                    SubBranch = branch.IsSubBranch == 1 ? "Yes" : "No"
                })
                .ToListAsync(cancellationToken);
        }

        public Task<BranchOffice?> FillBranchOfficeDetails(int branchOfficeId, CancellationToken cancellationToken)
        {
            return _portal.BranchOffices
                .AsNoTracking()
                .FirstOrDefaultAsync(branch => branch.BranchOfficeId == branchOfficeId, cancellationToken);
        }

        public async Task<BranchOfficeActionResult> SaveBranchOfficeDetails(BranchOfficeSaveRequest request, int modifiedBy, CancellationToken cancellationToken)
        {
            string headOfficeCode = (request.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            string partCode = (request.BranchPartCode ?? string.Empty).Trim().ToLowerInvariant();
            string name = (request.BranchOfficeName ?? string.Empty).Trim();

            if (headOfficeCode.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeRequired));
            }

            HeadOffice? headOffice = await _portal.HeadOffices
                .AsNoTracking()
                .FirstOrDefaultAsync(office => office.HeadOfficeCode == headOfficeCode, cancellationToken);
            if (headOffice == null || headOffice.Status != 2)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeMustBeActivated));
            }

            if (partCode.Length < 6 || partCode.Length > 10)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeLength));
            }

            if (string.Equals(partCode, headOfficeCode, StringComparison.OrdinalIgnoreCase))
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeSameAsHo));
            }

            if (name.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeNameRequired));
            }

            if (name.Length > 50)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeNameMax));
            }

            string fullCode = (headOfficeCode + partCode).ToLowerInvariant();
            bool duplicate = await _portal.BranchOffices.AnyAsync(
                branch => branch.BranchOfficeCode == fullCode && branch.BranchOfficeId != request.BranchOfficeId,
                cancellationToken);
            if (duplicate)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeExists));
            }

            if (request.BranchOfficeId == 0)
            {
                BranchOffice row = new BranchOffice
                {
                    HeadOfficeCode = headOfficeCode,
                    BranchPartCode = partCode,
                    BranchOfficeCode = fullCode,
                    BranchOfficeName = name,
                    DeploymentType = request.DeploymentType ?? 0,
                    PersonIncharge = TrimOrNull(request.PersonIncharge),
                    IsSubBranch = request.IsSubBranch == 1 ? 1 : 0,
                    CountryCode = string.IsNullOrWhiteSpace(request.CountryCode) ? "91" : request.CountryCode.Trim(),
                    MobileNo = TrimOrNull(request.MobileNo),
                    PhoneNo = TrimOrNull(request.PhoneNo),
                    BranchEmail = TrimOrNull(request.BranchEmail),
                    Address = TrimOrNull(request.Address),
                    City = TrimOrNull(request.City),
                    CountryId = request.CountryId,
                    StateId = request.StateId,
                    Pincode = TrimOrNull(request.Pincode),
                    ThirdPartyCode = TrimOrNull(request.ThirdPartyCode),
                    Status = 1,
                    UserCreatedStatus = 0,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = modifiedBy
                };

                _portal.BranchOffices.Add(row);
                await _portal.SaveChangesAsync(cancellationToken);
                return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeSaved), row.BranchOfficeId);
            }

            BranchOffice? existing = await _portal.BranchOffices.FirstOrDefaultAsync(
                branch => branch.BranchOfficeId == request.BranchOfficeId,
                cancellationToken);
            if (existing == null)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeNotFound));
            }

            existing.HeadOfficeCode = headOfficeCode;
            existing.BranchPartCode = partCode;
            existing.BranchOfficeCode = fullCode;
            existing.BranchOfficeName = name;
            existing.DeploymentType = request.DeploymentType ?? 0;
            existing.PersonIncharge = TrimOrNull(request.PersonIncharge);
            existing.IsSubBranch = request.IsSubBranch == 1 ? 1 : 0;
            existing.CountryCode = string.IsNullOrWhiteSpace(request.CountryCode) ? existing.CountryCode : request.CountryCode.Trim();
            existing.MobileNo = TrimOrNull(request.MobileNo);
            existing.PhoneNo = TrimOrNull(request.PhoneNo);
            existing.BranchEmail = TrimOrNull(request.BranchEmail);
            existing.Address = TrimOrNull(request.Address);
            existing.City = TrimOrNull(request.City);
            existing.CountryId = request.CountryId;
            existing.StateId = request.StateId;
            existing.Pincode = TrimOrNull(request.Pincode);
            existing.ThirdPartyCode = TrimOrNull(request.ThirdPartyCode);
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = modifiedBy;

            await _portal.SaveChangesAsync(cancellationToken);
            return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeSaved), existing.BranchOfficeId);
        }

        public async Task<BranchOfficeActionResult> UpdateOfficeStatus(string branchOfficeCode,int status, CancellationToken cancellationToken) 
        {
            string code = (branchOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0) 
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeRequired));
            }
            if (status < 1 || status > 3) 
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.StatusInvalid));
            }
            BranchOffice? existing = await _portal.BranchOffices.FirstOrDefaultAsync(branch => branch.BranchOfficeCode == code, cancellationToken);
            if (existing == null)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeNotFound));
            }

            existing.Status = status;
            existing.ModifiedDate = DateTime.UtcNow;
            await _portal.SaveChangesAsync(cancellationToken);

            string statusMessage = status == 1
                ? MessageCatalog.Office.BranchOfficeCreated
                : status == 2
                    ? MessageCatalog.Office.BranchOfficeActivated
                    : MessageCatalog.Office.BranchOfficeDeactivated;
            return Ok(MessageRender.GetMessage(statusMessage), existing.BranchOfficeId);
        }

        public async Task<BranchOfficeActionResult> DeleteBranchOfficeDetails(string branchOfficeCode, CancellationToken cancellationToken) 
        {
            string code = (branchOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeRequired));
            }
            BranchOffice? existing = await _portal.BranchOffices.FirstOrDefaultAsync(branch => branch.BranchOfficeCode == code, cancellationToken);
            if (existing == null) 
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeNotFound));    
            }
            _portal.BranchOffices.Remove(existing);
            await _portal.SaveChangesAsync(cancellationToken);
            return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDeleted), existing.BranchOfficeId);
        }



        #endregion
    }
}
