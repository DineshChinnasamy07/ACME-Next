/*  Class Name      : BranchOfficeTenantWriter.cs
 *  Purpose         : Dual-write Branch Office row, status, admin user, and delete into HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 10-Nov-2026
 */
using System.Security.Cryptography;
using Bosco.Common;
using Bosco.Organization.Application.Abstractions.Services;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;
using Bosco.Organization.Infrastructure.Persistence;
using Bosco.Organization.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Organization.Infrastructure.Services
{
    public class BranchOfficeTenantWriter : IBranchOfficeTenantWriter
    {
        private readonly IConfiguration _configuration;

        public BranchOfficeTenantWriter(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        #region Propertys
        private static BranchOfficeActionResult Ok(string message,int branchOfficeId) 
        {
            return new BranchOfficeActionResult { Success = true, Message = message, BranchOfficeId = branchOfficeId };
        }
        private static BranchOfficeActionResult Fail(string message)
        {
            return new BranchOfficeActionResult { Success = false, Message = message };
        }
        #endregion

        #region Methodes
        private BranchOfficeActionResult TryOpenTenant(HeadOffice headOffice,out OrganizationDbContext? tenant) 
        {
            tenant = null;
            if (headOffice == null) 
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeDbMissingForBranch));
            }
            string host = (headOffice.HostName ?? string.Empty).Trim();
            string database = (headOffice.DbName ?? string.Empty).Trim();
            string user = (headOffice.UserName ?? string.Empty).Trim();
            string password = headOffice.Password ?? string.Empty;
            if (host.Length == 0 || database.Length == 0 || user.Length == 0) 
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeDbMissingForBranch));
            }
            string connectionString = BuildTenantConnectionString(host,database,user,password);
            DbContextOptionsBuilder<OrganizationDbContext> optionBuilder = new DbContextOptionsBuilder<OrganizationDbContext>();
            optionBuilder.UseNpgsql(connectionString);
            tenant = new OrganizationDbContext(optionBuilder.Options);
            return new BranchOfficeActionResult { Success = true };
        }

        private string BuildTenantConnectionString(string host,string database,string user,string password) 
        {
            string portText = _configuration["HeadOfficeTenant:Port"] ?? "3308";
            int port;
            if (!int.TryParse(portText, out port) || port <= 0) 
            {
                port = 3308;
            }
            return "Host=" + host + ";Port=" + port.ToString(System.Globalization.CultureInfo.InvariantCulture)
                   + ";Database=" + database
                   + ";Username=" + user
                   + ";Password=" + password;
        }
        //New tenant row,BranchOfficeId stays 0 so the tenant DB makes its own identity.
        private static BranchOffice CopyToNewRow(BranchOffice source) 
        {
            BranchOffice row = new BranchOffice();
            ApplyFields(row, source);
            row.BranchOfficeId = 0;
            row.CreatedDate = source.CreatedDate;
            row.CreatedBy = source.CreatedBy;
            // BRANCH_KEY_CODE is NOT NULL in the head office table; the portal row keeps it empty until a license is generated.
            row.BranchKeyCode = source.BranchKeyCode ?? string.Empty;
            return row;
        }
        // The head office branch_office table has NOT NULL columns, so empty portal values become "" or 0 here.
        private static string? Truncate(string? value, int maxLength)
        {
            if (value == null || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength);
        }

        //Field map from HO-BO - tasks.txt.BranchOfficeId is not copied.
        private static void ApplyFields(BranchOffice target, BranchOffice source) 
        {
            target.BranchOfficeCode = (source.BranchOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            target.HeadOfficeCode = (source.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            target.BranchPartCode = source.BranchPartCode ?? string.Empty;
            target.BranchOfficeName = source.BranchOfficeName ?? string.Empty;
            target.DeploymentType = source.DeploymentType ?? 0;
            target.PersonIncharge = source.PersonIncharge ?? string.Empty;
            target.IsSubBranch = source.IsSubBranch;
            target.CountryCode = source.CountryCode ?? string.Empty;
            target.MobileNo = source.MobileNo ?? string.Empty;
            target.PhoneNo = source.PhoneNo ?? string.Empty;
            target.BranchEmail = source.BranchEmail ?? string.Empty;
            target.Address = source.Address ?? string.Empty;
            target.City = source.City ?? string.Empty;
            target.CountryId = source.CountryId ?? 0;
            target.StateId = source.StateId ?? 0;
            target.Pincode = source.Pincode ?? string.Empty;
            target.ThirdPartyCode = Truncate(source.ThirdPartyCode, 12);
            target.Status = source.Status;
            target.UserCreatedStatus = source.UserCreatedStatus ?? 0;
            target.BranchKeyCode = source.BranchKeyCode ?? string.Empty;
            target.ModifiedDate = source.ModifiedDate;
            target.ModifiedBy = source.ModifiedBy;
        }

        // HO branch_office has FK to head_office. Empty tenant head_office blocks every branch insert.
        private static async Task EnsureTenantHeadOfficeExists(OrganizationDbContext tenant, HeadOffice portalOffice, CancellationToken cancellationToken)
        {
            string code = (portalOffice.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return;
            }

            bool exists = await tenant.HeadOffices.AnyAsync(
                office => office.HeadOfficeCode == code,
                cancellationToken);
            if (exists)
            {
                return;
            }

            HeadOffice row = new HeadOffice
            {
                HeadOfficeId = 0,
                HeadOfficeCode = code,
                HeadOfficeName = portalOffice.HeadOfficeName ?? code,
                TypeId = portalOffice.TypeId ?? 0,
                BelongsTo = portalOffice.BelongsTo ?? string.Empty,
                Designation = portalOffice.Designation ?? string.Empty,
                OrgMailId = portalOffice.OrgMailId ?? string.Empty,
                InchargeName = portalOffice.InchargeName ?? string.Empty,
                Address = portalOffice.Address ?? string.Empty,
                CreatedDate = portalOffice.CreatedDate == default ? DateTime.UtcNow : portalOffice.CreatedDate,
                CreatedBy = portalOffice.CreatedBy ?? 0,
                ModifiedDate = portalOffice.ModifiedDate,
                ModifiedBy = portalOffice.ModifiedBy,
                Status = portalOffice.Status ?? 2,
                City = portalOffice.City ?? string.Empty,
                HostName = null,
                DbName = null,
                UserName = null,
                Password = null,
                UserCreatedStatus = portalOffice.UserCreatedStatus ?? 0,
                CountryId = portalOffice.CountryId ?? 0,
                StateId = portalOffice.StateId ?? 0,
                Pincode = portalOffice.Pincode ?? string.Empty,
                SrInchargeName = portalOffice.SrInchargeName ?? string.Empty,
                SrMobileNo = portalOffice.SrMobileNo ?? string.Empty,
                SrPhoneNo = portalOffice.SrPhoneNo ?? string.Empty,
                SrMailId = portalOffice.SrMailId ?? string.Empty,
                MobileNo = portalOffice.MobileNo ?? string.Empty,
                PhoneNo = portalOffice.PhoneNo ?? string.Empty,
                CountryCode = portalOffice.CountryCode ?? string.Empty,
                AccountingYearType = portalOffice.AccountingYearType
            };
            tenant.HeadOffices.Add(row);
            await tenant.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Implementation of IBranchOfficeTenantWriter
        public async Task<BranchOfficeActionResult> SaveBranchOfficeDetails(HeadOffice headOffice, BranchOffice portalBranch, CancellationToken cancellationToken)
        {
            if (portalBranch == null)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDetailsRequired));
            }
            OrganizationDbContext? tenant;
            BranchOfficeActionResult open = TryOpenTenant(headOffice, out tenant);
            if (!open.Success || tenant == null)
            {
                return open;
            }
            try
            {
                await using (tenant)
                {
                    await EnsureTenantHeadOfficeExists(tenant, headOffice, cancellationToken);
                    string code = (portalBranch.BranchOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
                    if (code.Length == 0)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeRequired));
                    }
                    BranchOffice? existing = await tenant.BranchOffices.FirstOrDefaultAsync(
                        branch => branch.BranchOfficeCode == code,
                        cancellationToken);
                    if (existing == null)
                    {
                        BranchOffice row = CopyToNewRow(portalBranch);
                        tenant.BranchOffices.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDualWriteSaved), row.BranchOfficeId);
                    }
                    ApplyFields(existing, portalBranch);
                    existing.ModifiedDate = DateTime.UtcNow;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDualWriteSaved), existing.BranchOfficeId);
                }
            }
            catch (Exception ex)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeSyncFailed, ex.Message));
            }
        }

        public async Task<BranchOfficeActionResult> UpdateOfficeStatus(HeadOffice headOffice, BranchOffice portalBranch, int status, CancellationToken cancellationToken)
        {
            if (portalBranch == null)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDetailsRequired));
            }
            OrganizationDbContext? tenant;
            BranchOfficeActionResult open = TryOpenTenant(headOffice, out tenant);
            if (!open.Success || tenant == null)
            {
                return open;
            }
            try
            {
                await using (tenant)
                {
                    await EnsureTenantHeadOfficeExists(tenant, headOffice, cancellationToken);
                    string code = (portalBranch.BranchOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
                    if (code.Length == 0)
                    {
                        return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeRequired));
                    }
                    BranchOffice? existing = await tenant.BranchOffices.FirstOrDefaultAsync(
                        branch => branch.BranchOfficeCode == code,
                        cancellationToken);
                    if (existing == null)
                    {
                        BranchOffice row = CopyToNewRow(portalBranch);
                        row.Status = status;
                        tenant.BranchOffices.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeStatusDualWriteSaved), row.BranchOfficeId);
                    }
                    existing.Status = status;
                    existing.ModifiedDate = DateTime.UtcNow;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeStatusDualWriteSaved), existing.BranchOfficeId);
                }
            }
            catch (Exception ex)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeSyncFailed, ex.Message));
            }
        }

        public async Task<BranchOfficeActionResult> DeleteBranchOfficeDetails(HeadOffice headOffice, string branchOfficeCode, CancellationToken cancellationToken) 
        {
            string code = (branchOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeRequired));
            }
            OrganizationDbContext? tenant;
            BranchOfficeActionResult open = TryOpenTenant(headOffice, out tenant);
            if (!open.Success || tenant == null)
            {
                return open;
            }
            try
            {
                await using (tenant)
                {
                    BranchOffice? existing = await tenant.BranchOffices.FirstOrDefaultAsync(
                        branch => branch.BranchOfficeCode == code,
                        cancellationToken);
                    if (existing == null)
                    {
                        return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDeleteDualWriteSaved), 0);
                    }
                    int tenantId = existing.BranchOfficeId;
                    tenant.BranchOffices.Remove(existing);
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDeleteDualWriteSaved), tenantId);
                }
            }
            catch (Exception ex)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeSyncFailed, ex.Message));
            }
        }

        public async Task<BranchOfficeActionResult> EnsureBranchOfficeAdminUser(HeadOffice headOffice, BranchOffice portalBranch, CancellationToken cancellationToken)
        {
            if (portalBranch == null)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeDetailsRequired));
            }

            OrganizationDbContext? tenant;
            BranchOfficeActionResult open = TryOpenTenant(headOffice, out tenant);
            if (!open.Success || tenant == null)
            {
                return open;
            }

            string headOfficeCode = (portalBranch.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            string branchCode = (portalBranch.BranchOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            string userName = (portalBranch.BranchPartCode ?? string.Empty).Trim().ToLowerInvariant();
            if (headOfficeCode.Length == 0 || branchCode.Length == 0 || userName.Length == 0)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeCodeRequired));
            }

            string plainPassword = CreateRandomPassword();
            string encryptedPassword = LegacyPasswordCipher.EncryptValue(plainPassword);

            try
            {
                await using (tenant)
                {
                    bool exists = await tenant.TenantUsers.AnyAsync(
                        user => user.UserName == userName && user.HeadOfficeCode == headOfficeCode,
                        cancellationToken);
                    if (exists)
                    {
                        return new BranchOfficeActionResult
                        {
                            Success = true,
                            Message = MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeAdminExists),
                            BranchOfficeId = portalBranch.BranchOfficeId
                        };
                    }

                    TenantUser row = new TenantUser
                    {
                        FirstName = branchCode,
                        UserName = userName,
                        Password = encryptedPassword,
                        EmailId = (portalBranch.BranchEmail ?? string.Empty).Trim().ToLowerInvariant(),
                        ContactNo = (portalBranch.MobileNo ?? string.Empty).Trim(),
                        RoleId = 2,
                        UserType = 2,
                        Status = 1,
                        PasswordStatus = 0,
                        HeadOfficeCode = headOfficeCode,
                        BranchCode = branchCode,
                        CreatedBy = 0
                    };

                    tenant.TenantUsers.Add(row);
                    await tenant.SaveChangesAsync(cancellationToken);

                    return new BranchOfficeActionResult
                    {
                        Success = true,
                        Message = MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeAdminCreated),
                        BranchOfficeId = portalBranch.BranchOfficeId,
                        GeneratedAdminPassword = plainPassword
                    };
                }
            }
            catch (Exception ex)
            {
                return Fail(MessageRender.GetMessage(MessageCatalog.Office.BranchOfficeAdminCreateFailed, ex.Message));
            }
        }

        private static string CreateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            char[] buffer = new char[10];
            byte[] bytes = new byte[10];
            RandomNumberGenerator.Fill(bytes);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = chars[bytes[i] % chars.Length];
            }

            return new string(buffer);
        }

        #endregion
    }
}
