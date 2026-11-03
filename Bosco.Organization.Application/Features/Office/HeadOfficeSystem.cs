/*  Class Name      : HeadOfficeSystem.cs
 *  Purpose         : Portal Head Office use cases (list, get, save, status, delete, dual-write)
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Organization.Application.Abstractions.Repositories;
using Bosco.Organization.Application.Abstractions.Services;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;

namespace Bosco.Organization.Application.Features.Office
{
    public class HeadOfficeSystem
    {
        private readonly IHeadOfficeRepository _headOffices;
        private readonly IHeadOfficeTenantWriter _tenantWriter;
        private readonly IHeadOfficeDatabaseRestorer _databaseRestorer;
        public HeadOfficeSystem( IHeadOfficeRepository headOffices,IHeadOfficeTenantWriter tenantWriter,IHeadOfficeDatabaseRestorer databaseRestorer)
        {
            _headOffices = headOffices;
            _tenantWriter = tenantWriter;
            _databaseRestorer = databaseRestorer;
        }
        #region Head Office Operations

        public Task<IReadOnlyList<HeadOfficeListItemDto>> FetchHeadOfficeDetails(CancellationToken cancellationToken)
        {
            return _headOffices.FetchHeadOfficeDetails(cancellationToken);
        }

        public Task<HeadOffice?> HeadOfficeDetailById(int headOfficeId, CancellationToken cancellationToken)
        {
            if (headOfficeId <= 0)
            {
                return Task.FromResult<HeadOffice?>(null);
            }

            return _headOffices.HeadOfficeDetailById(headOfficeId, cancellationToken);
        }



        public async Task<HeadOfficeActionResult> SaveHeadOfficeDetails(HeadOfficeSaveRequest request, int modifiedBy, CancellationToken cancellationToken) 
        {
            if (request == null)
            {
                return new HeadOfficeActionResult
                {
                    Success = false,
                    Message = "Head Office details are required."
                };
            }
            if (request.HeadOfficeId == 0)
            {
                HeadOfficeActionResult restore = await _databaseRestorer.RestoreACPERPdatabase(
                    request.HostName ?? string.Empty,
                    request.DbName ?? string.Empty,
                    request.UserName ?? string.Empty,
                    request.Password ?? string.Empty,
                    cancellationToken);
                if (!restore.Success)
                {
                    return restore;
                }
            }
            HeadOfficeActionResult portalResult = await _headOffices.SaveHeadOfficeDetails(request, modifiedBy, cancellationToken);
            if (!portalResult.Success)
            {
                return portalResult;
            }

            HeadOffice? portalOffice = await _headOffices.HeadOfficeDetailById(portalResult.HeadOfficeId, cancellationToken);
            if (portalOffice == null)
            {
                return new HeadOfficeActionResult
                {
                    Success = true,
                    Message = "Head Office Saved on portal. HO database sync skipped (portal row not found).",
                    HeadOfficeId = portalResult.HeadOfficeId
                };
            }



            HeadOfficeActionResult tenantResult = await _tenantWriter.SaveHeadOfficeDetails(portalOffice, cancellationToken);
            if (!tenantResult.Success)
            {
                return new HeadOfficeActionResult
                {
                    Success = true,
                    Message = "Head Office Saved on portal. " + tenantResult.Message,
                    HeadOfficeId = portalResult.HeadOfficeId
                };
            }

            return new HeadOfficeActionResult
            {
                Success = true,
                Message = "Head Office Saved.",
                HeadOfficeId = portalResult.HeadOfficeId
            };
        }

        public async Task<HeadOfficeActionResult> UpdateOfficeStatus(string headOfficeCode, int status, CancellationToken cancellationToken)
        {
            if (status == 3)
            {
                IReadOnlyList<BranchByHeadOfficeDto> branches = await _headOffices.FetchBranchByHeadOffice(headOfficeCode, cancellationToken);
                if (branches.Count > 0)
                {
                    return new HeadOfficeActionResult
                    {
                        Success = false,
                        Message = "Head Office cannot be deactivated"
                    };
                }
            }

            HeadOffice? before = await _headOffices.HeadOfficeDetailByCode(headOfficeCode, cancellationToken);
            int previousStatus = before == null || before.Status == null ? 0 : before.Status.Value;

            HeadOfficeActionResult portalResult = await _headOffices.UpdateOfficeStatus(headOfficeCode, status, cancellationToken);
            if (!portalResult.Success)
            {
                return portalResult;
            }

            HeadOffice? portalOffice = await _headOffices.HeadOfficeDetailByCode(headOfficeCode, cancellationToken);
            if (portalOffice == null)
            {
                return portalResult;
            }

            HeadOfficeActionResult tenantStatus = await _tenantWriter.UpdateOfficeStatus(portalOffice, status, cancellationToken);
            string message = portalResult.Message;
            if (!tenantStatus.Success)
            {
                message = message + " " + tenantStatus.Message;
            }

            string? generatedPassword = null;
            if (previousStatus == 1 && status == 2)
            {
                HeadOfficeActionResult adminResult = await _tenantWriter.EnsureHeadOfficeAdminUser(portalOffice, cancellationToken);
                if (adminResult.Success)
                {
                    generatedPassword = adminResult.GeneratedAdminPassword;
                    message = message + " " + adminResult.Message;
                    await _headOffices.UpdateUserCreatedStatus(headOfficeCode, 2, cancellationToken);
                    portalOffice.UserCreatedStatus = 2;
                    await _tenantWriter.SaveHeadOfficeDetails(portalOffice, cancellationToken);
                }
                else
                {
                    message = message + " " + adminResult.Message;
                }
            }

            return new HeadOfficeActionResult
            {
                Success = true,
                Message = message.Trim(),
                HeadOfficeId = portalResult.HeadOfficeId,
                GeneratedAdminPassword = generatedPassword
            };
        }

        public Task<HeadOfficeActionResult> DeleteHeadOfficeDetails(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _headOffices.DeleteHeadOfficeDetails(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<ActiveHeadOfficeDto>> FetchActiveHeadOfficeDetails(CancellationToken cancellationToken)
        {
            return _headOffices.FetchActiveHeadOfficeDetails(cancellationToken);
        }

        public Task<IReadOnlyList<HeadOfficeTypeDto>> FetchHeadOfficeType(CancellationToken cancellationToken)
        {
            return _headOffices.FetchHeadOfficeType(cancellationToken);
        }

        public Task<IReadOnlyList<CountryDto>> FetchCountry(CancellationToken cancellationToken)
        {
            return _headOffices.FetchCountry(cancellationToken);
        }

        public Task<IReadOnlyList<StateDto>> FetchStateByCountry(int countryId, CancellationToken cancellationToken)
        {
            if (countryId <= 0)
            {
                return Task.FromResult<IReadOnlyList<StateDto>>(Array.Empty<StateDto>());
            }

            return _headOffices.FetchStateByCountry(countryId, cancellationToken);
        }

        public Task<IReadOnlyList<BranchByHeadOfficeDto>> FetchBranchByHeadOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _headOffices.FetchBranchByHeadOffice(headOfficeCode, cancellationToken);
        }

        public Task<HeadOfficeActionResult> UpdateDatabase(string headOfficeCode, UpdateDatabaseRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new HeadOfficeActionResult
                {
                    Success = false,
                    Message = "Database details are required."
                });
            }

            return _headOffices.UpdateDatabase(headOfficeCode, request, cancellationToken);
        }

        public Task<HeadOfficeActionResult> UpdateUserCreatedStatus(string headOfficeCode, int userCreatedStatus, CancellationToken cancellationToken)
        {
            return _headOffices.UpdateUserCreatedStatus(headOfficeCode, userCreatedStatus, cancellationToken);
        }

        #endregion
    }
}
