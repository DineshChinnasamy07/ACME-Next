/*  Class Name      : HeadOfficeRepository.cs
 *  Purpose         : Portal head_office list, get, save, status, delete
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Common;
using Bosco.Organization.Application.Abstractions.Repositories;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;
using Bosco.Organization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bosco.Organization.Infrastructure.Repositories
{
    public class HeadOfficeRepository : IHeadOfficeRepository
    {
        private readonly OrganizationDbContext _portal;

        public HeadOfficeRepository(OrganizationDbContext portal)
        {
            _portal = portal;
        }

        #region Methode
        private static string? TrimOrNull(string? value)
        {
            if (value == null)
            {
                return null;
            }

            string trimmed = value.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }
        #endregion

        #region Implemantion
       
        public async Task<IReadOnlyList<HeadOfficeListItemDto>> FetchHeadOfficeDetails(CancellationToken cancellationToken)
        {
            return await _portal.HeadOffices
                .AsNoTracking()
                .OrderBy(office => office.Status)
                .ThenBy(office => office.HeadOfficeCode)
                .Select(office => new HeadOfficeListItemDto
                {
                    HeadOfficeId = office.HeadOfficeId,
                    Code = office.HeadOfficeCode,
                    Name = office.HeadOfficeName ?? string.Empty,
                    BelongsTo = office.BelongsTo,
                    TypeId = office.TypeId,
                    Status = office.Status ?? 0,
                    HoStatus = office.Status == 1 ? "Created"
                        : office.Status == 2 ? "Activated"
                        : "DeActivated"
                })
                .ToListAsync(cancellationToken);
        }

        public Task<HeadOffice?> HeadOfficeDetailById(int headOfficeId, CancellationToken cancellationToken)
        {
            return _portal.HeadOffices
                .AsNoTracking()
                .FirstOrDefaultAsync(office => office.HeadOfficeId == headOfficeId, cancellationToken);
        }

        public Task<HeadOffice?> HeadOfficeDetailByCode(string headOfficeCode, CancellationToken cancellationToken)
        {
            string code = (headOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return Task.FromResult<HeadOffice?>(null);
            }

            return _portal.HeadOffices
                .AsNoTracking()
                .FirstOrDefaultAsync(office => office.HeadOfficeCode == code, cancellationToken);
        }

        public async Task<HeadOfficeActionResult> SaveHeadOfficeDetails(HeadOfficeSaveRequest request, int modifiedBy, CancellationToken cancellationToken)
        {
            string code = (request.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            string name = (request.HeadOfficeName ?? string.Empty).Trim();

            if (code.Length == 0 || code.Length > 6)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeCodeMax) };
            }

            if (name.Length == 0)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNameRequired) };
            }

            bool duplicate = await _portal.HeadOffices.AnyAsync(
                office => office.HeadOfficeCode == code && office.HeadOfficeId != request.HeadOfficeId,
                cancellationToken);
            if (duplicate)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeCodeExists) };
            }

            if (request.HeadOfficeId == 0)
            {
                HeadOffice row = new HeadOffice
                {
                    HeadOfficeCode = code,
                    HeadOfficeName = name,
                    TypeId = request.TypeId,
                    BelongsTo = TrimOrNull(request.BelongsTo),
                    Designation = TrimOrNull(request.Designation),
                    OrgMailId = TrimOrNull(request.OrgMailId),
                    InchargeName = TrimOrNull(request.InchargeName),
                    Address = TrimOrNull(request.Address),
                    City = TrimOrNull(request.City),
                    MobileNo = TrimOrNull(request.MobileNo),
                    PhoneNo = TrimOrNull(request.PhoneNo),
                    CountryId = request.CountryId,
                    StateId = request.StateId,
                    Pincode = TrimOrNull(request.Pincode),
                    CountryCode = TrimOrNull(request.CountryCode),
                    SrInchargeName = TrimOrNull(request.SrInchargeName),
                    SrMobileNo = TrimOrNull(request.SrMobileNo),
                    SrPhoneNo = TrimOrNull(request.SrPhoneNo),
                    SrMailId = TrimOrNull(request.SrMailId),
                    HostName = TrimOrNull(request.HostName),
                    DbName = TrimOrNull(request.DbName),
                    UserName = TrimOrNull(request.UserName),
                    Password = TrimOrNull(request.Password),
                    Status = 1,
                    UserCreatedStatus = 0,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = modifiedBy,
                    AccountingYearType = request.AccountingYearType
                };

                _portal.HeadOffices.Add(row);
                await _portal.SaveChangesAsync(cancellationToken);
                return new HeadOfficeActionResult
                {
                    Success = true,
                    Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeSaved),
                    HeadOfficeId = row.HeadOfficeId
                };
            }

            HeadOffice? existing = await _portal.HeadOffices.FirstOrDefaultAsync(
                office => office.HeadOfficeId == request.HeadOfficeId,
                cancellationToken);
            if (existing == null)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) };
            }

            existing.HeadOfficeCode = code;
            existing.HeadOfficeName = name;
            existing.TypeId = request.TypeId;
            existing.BelongsTo = TrimOrNull(request.BelongsTo);
            existing.Designation = TrimOrNull(request.Designation);
            existing.OrgMailId = TrimOrNull(request.OrgMailId);
            existing.InchargeName = TrimOrNull(request.InchargeName);
            existing.Address = TrimOrNull(request.Address);
            existing.City = TrimOrNull(request.City);
            existing.MobileNo = TrimOrNull(request.MobileNo);
            existing.PhoneNo = TrimOrNull(request.PhoneNo);
            existing.CountryId = request.CountryId;
            existing.StateId = request.StateId;
            existing.Pincode = TrimOrNull(request.Pincode);
            existing.CountryCode = TrimOrNull(request.CountryCode);
            existing.SrInchargeName = TrimOrNull(request.SrInchargeName);
            existing.SrMobileNo = TrimOrNull(request.SrMobileNo);
            existing.SrPhoneNo = TrimOrNull(request.SrPhoneNo);
            existing.SrMailId = TrimOrNull(request.SrMailId);
            existing.AccountingYearType = request.AccountingYearType;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = modifiedBy;

            if (!string.IsNullOrWhiteSpace(request.HostName))
            {
                existing.HostName = request.HostName.Trim();
            }
            if (!string.IsNullOrWhiteSpace(request.DbName))
            {
                existing.DbName = request.DbName.Trim();
            }
            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                existing.UserName = request.UserName.Trim();
            }
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                existing.Password = request.Password.Trim();
            }

            await _portal.SaveChangesAsync(cancellationToken);
            return new HeadOfficeActionResult
            {
                Success = true,
                Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeSaved),
                HeadOfficeId = existing.HeadOfficeId
            };
        }

        public async Task<HeadOfficeActionResult> UpdateOfficeStatus(string headOfficeCode, int status, CancellationToken cancellationToken)
        {
            string code = (headOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeCodeRequired) };
            }

            if (status < 1 || status > 3)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.StatusInvalid) };
            }

            HeadOffice? existing = await _portal.HeadOffices.FirstOrDefaultAsync(
                office => office.HeadOfficeCode == code,
                cancellationToken);
            if (existing == null)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) };
            }

            existing.Status = status;
            existing.ModifiedDate = DateTime.UtcNow;
            await _portal.SaveChangesAsync(cancellationToken);

            string statusKey = status == 1
                ? MessageCatalog.Office.HeadOfficeCreated
                : status == 2
                    ? MessageCatalog.Office.HeadOfficeActivated
                    : MessageCatalog.Office.HeadOfficeDeactivated;
            return new HeadOfficeActionResult
            {
                Success = true,
                Message = MessageRender.GetMessage(statusKey),
                HeadOfficeId = existing.HeadOfficeId
            };
        }

        public async Task<HeadOfficeActionResult> DeleteHeadOfficeDetails(string headOfficeCode, CancellationToken cancellationToken)
        {
            string code = (headOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeCodeRequired) };
            }

            HeadOffice? existing = await _portal.HeadOffices.FirstOrDefaultAsync(
                office => office.HeadOfficeCode == code,
                cancellationToken);
            if (existing == null)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) };
            }

            if (existing.Status == 2)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeCannotDeleteActivated) };
            }

            _portal.HeadOffices.Remove(existing);
            await _portal.SaveChangesAsync(cancellationToken);
            return new HeadOfficeActionResult
            {
                Success = true,
                Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeDeleted),
                HeadOfficeId = existing.HeadOfficeId
            };
        }

        public async Task<IReadOnlyList<ActiveHeadOfficeDto>> FetchActiveHeadOfficeDetails(CancellationToken cancellationToken)
        {
            return await _portal.HeadOffices
                .AsNoTracking()
                .Where(office => office.Status == 2)
                .OrderBy(office => office.HeadOfficeCode)
                .Select(office => new ActiveHeadOfficeDto
                {
                    HeadOfficeId = office.HeadOfficeId,
                    HeadOfficeCode = office.HeadOfficeCode.ToLower()
                }).ToListAsync(cancellationToken);        
        }

        public async Task<IReadOnlyList<HeadOfficeTypeDto>> FetchHeadOfficeType(CancellationToken cancellationToken)
        {
            return await _portal.HeadOfficeTypes
                .AsNoTracking()
                .OrderBy(row => row.Type)
                .Select(row => new HeadOfficeTypeDto
                {
                    TypeId = row.TypeId,
                    Type = row.Type
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CountryDto>> FetchCountry(CancellationToken cancellationToken)
        {
            return await _portal.Countries
                .AsNoTracking()
                .OrderBy(row => row.CountryName)
                .Select(row => new CountryDto
                {
                    CountryId = row.CountryId,
                    CountryCode = row.CountryCode,
                    Country = row.CountryName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<StateDto>> FetchStateByCountry(int countryId, CancellationToken cancellationToken)
        {
            return await _portal.States
                .AsNoTracking()
                .Where(row => row.CountryId == countryId)
                .OrderBy(row => row.StateName)
                .Select(row => new StateDto
                {
                    StateId = row.StateId,
                    State = row.StateName,
                    StateCode = row.StateCode
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<BranchByHeadOfficeDto>> FetchBranchByHeadOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            string code = (headOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return Array.Empty<BranchByHeadOfficeDto>();
            }

            return await _portal.BranchOffices
                .AsNoTracking()
                .Where(branch =>
                    branch.HeadOfficeCode.ToLower() == code
                    && branch.Status == 2
                    && branch.IsSubBranch == 0)
                .OrderBy(branch => branch.BranchOfficeName)
                .Select(branch => new BranchByHeadOfficeDto
                {
                    BranchOfficeId = branch.BranchOfficeId,
                    BranchOfficeCode = branch.BranchOfficeCode.ToLower(),
                    BranchOfficeName = branch.BranchOfficeName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<HeadOfficeActionResult> UpdateDatabase(string headOfficeCode, UpdateDatabaseRequest request, CancellationToken cancellationToken)
        {
            string code = (headOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0) 
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeCodeRequired) };
            }
            if (request == null) 
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.DatabaseDetailsRequired) };
            }

            HeadOffice? existing = await _portal.HeadOffices.FirstOrDefaultAsync(
                office => office.HeadOfficeCode == code, cancellationToken);
            if (existing == null)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) };
            }

            existing.HostName = TrimOrNull(request.HostName);
            existing.DbName = TrimOrNull(request.DbName);
            existing.UserName = TrimOrNull(request.UserName);
            existing.Password = TrimOrNull(request.Password);
            existing.ModifiedDate = DateTime.UtcNow;

            await _portal.SaveChangesAsync(cancellationToken);
            return new HeadOfficeActionResult
            {
                Success = true,
                Message = MessageRender.GetMessage(MessageCatalog.Office.DatabaseInfoUpdated),
                HeadOfficeId = existing.HeadOfficeId
            };
        }

        public async Task<HeadOfficeActionResult> UpdateUserCreatedStatus(string headOfficeCode, int userCreatedStatus, CancellationToken cancellationToken)
        {
            string code = (headOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeCodeRequired) };
            }

            if (userCreatedStatus < 0 || userCreatedStatus > 3)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.UserCreatedStatusInvalid) };
            }

            HeadOffice? existing = await _portal.HeadOffices.FirstOrDefaultAsync(
                office => office.HeadOfficeCode == code,
                cancellationToken);
            if (existing == null)
            {
                return new HeadOfficeActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) };
            }

            existing.UserCreatedStatus = userCreatedStatus;
            existing.ModifiedDate = DateTime.UtcNow;
            await _portal.SaveChangesAsync(cancellationToken);

            return new HeadOfficeActionResult
            {
                Success = true,
                Message = MessageRender.GetMessage(MessageCatalog.Office.UserCreatedStatusUpdated),
                HeadOfficeId = existing.HeadOfficeId
            };
        }

        #endregion
    }
}
