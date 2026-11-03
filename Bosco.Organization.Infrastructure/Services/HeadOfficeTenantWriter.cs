/*  Class Name      : HeadOfficeTenantWriter.cs
 *  Purpose         : Dual-write HO row, status, and admin user into HO tenant DB
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using System.Security.Cryptography;
using Bosco.Organization.Application.Abstractions.Services;
using Bosco.Organization.Application.Dtos;
using Bosco.Organization.Domain.Entities;
using Bosco.Organization.Infrastructure.Persistence;
using Bosco.Organization.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Organization.Infrastructure.Services
{
    public class HeadOfficeTenantWriter : IHeadOfficeTenantWriter
    {
        private readonly IConfiguration _configuration;

        public HeadOfficeTenantWriter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HeadOfficeActionResult> SaveHeadOfficeDetails(HeadOffice portalOffice, CancellationToken cancellationToken)
        {
            OrganizationDbContext? tenant;
            HeadOfficeActionResult open = TryOpenTenant(portalOffice, out tenant);
            if (!open.Success || tenant == null)
            {
                return open;
            }

            try
            {
                await using (tenant)
                {
                    string code = (portalOffice.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
                    HeadOffice? existing = await tenant.HeadOffices.FirstOrDefaultAsync(
                        office => office.HeadOfficeCode == code,
                        cancellationToken);

                    if (existing == null)
                    {
                        HeadOffice row = CopyWithoutSecrets(portalOffice);
                        row.HeadOfficeId = 0;
                        tenant.HeadOffices.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok("Head Office dual-write saved.", row.HeadOfficeId);
                    }

                    ApplyWithoutSecrets(existing, portalOffice);
                    existing.ModifiedDate = DateTime.UtcNow;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok("Head Office dual-write saved.", existing.HeadOfficeId);
                }
            }
            catch (Exception ex)
            {
                return Fail("HO database sync failed: " + ex.Message);
            }
        }

        public async Task<HeadOfficeActionResult> UpdateOfficeStatus(HeadOffice portalOffice, int status, CancellationToken cancellationToken)
        {
            OrganizationDbContext? tenant;
            HeadOfficeActionResult open = TryOpenTenant(portalOffice, out tenant);
            if (!open.Success || tenant == null)
            {
                return open;
            }

            try
            {
                await using (tenant)
                {
                    string code = (portalOffice.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
                    HeadOffice? existing = await tenant.HeadOffices.FirstOrDefaultAsync(
                        office => office.HeadOfficeCode == code,
                        cancellationToken);

                    if (existing == null)
                    {
                        HeadOffice row = CopyWithoutSecrets(portalOffice);
                        row.HeadOfficeId = 0;
                        row.Status = status;
                        tenant.HeadOffices.Add(row);
                        await tenant.SaveChangesAsync(cancellationToken);
                        return Ok("Head Office status dual-write saved.", row.HeadOfficeId);
                    }

                    existing.Status = status;
                    existing.ModifiedDate = DateTime.UtcNow;
                    await tenant.SaveChangesAsync(cancellationToken);
                    return Ok("Head Office status dual-write saved.", existing.HeadOfficeId);
                }
            }
            catch (Exception ex)
            {
                return Fail("HO status sync failed: " + ex.Message);
            }
        }

        public async Task<HeadOfficeActionResult> EnsureHeadOfficeAdminUser(HeadOffice portalOffice, CancellationToken cancellationToken)
        {
            OrganizationDbContext? tenant;
            HeadOfficeActionResult open = TryOpenTenant(portalOffice, out tenant);
            if (!open.Success || tenant == null)
            {
                return open;
            }

            string code = (portalOffice.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            if (code.Length == 0)
            {
                return Fail("Head Office Code is required.");
            }

            string plainPassword = CreateRandomPassword();
            string encryptedPassword = LegacyPasswordCipher.EncryptValue(plainPassword);

            try
            {
                await using (tenant)
                {
                    bool exists = await tenant.TenantUsers.AnyAsync(
                        user => user.UserName == code && user.HeadOfficeCode == code,
                        cancellationToken);
                    if (exists)
                    {
                        return new HeadOfficeActionResult
                        {
                            Success = true,
                            Message = "Head Office admin user already exists.",
                            HeadOfficeId = portalOffice.HeadOfficeId
                        };
                    }

                    TenantUser row = new TenantUser
                    {
                        FirstName = code,
                        UserName = code,
                        Password = encryptedPassword,
                        EmailId = (portalOffice.OrgMailId ?? string.Empty).Trim().ToLowerInvariant(),
                        ContactNo = (portalOffice.MobileNo ?? string.Empty).Trim(),
                        RoleId = 1,
                        UserType = 1,
                        Status = 1,
                        PasswordStatus = 0,
                        HeadOfficeCode = code,
                        BranchCode = null,
                        CreatedBy = 0
                    };

                    tenant.TenantUsers.Add(row);
                    await tenant.SaveChangesAsync(cancellationToken);

                    return new HeadOfficeActionResult
                    {
                        Success = true,
                        Message = "Head Office admin user created.",
                        HeadOfficeId = portalOffice.HeadOfficeId,
                        GeneratedAdminPassword = plainPassword
                    };
                }
            }
            catch (Exception ex)
            {
                return Fail("HO admin user create failed: " + ex.Message);
            }
        }

        private HeadOfficeActionResult TryOpenTenant(HeadOffice portalOffice, out OrganizationDbContext? tenant)
        {
            tenant = null;
            if (portalOffice == null)
            {
                return Fail("Head Office details are required.");
            }

            string host = (portalOffice.HostName ?? string.Empty).Trim();
            string database = (portalOffice.DbName ?? string.Empty).Trim();
            string user = (portalOffice.UserName ?? string.Empty).Trim();
            string password = portalOffice.Password ?? string.Empty;

            if (host.Length == 0 || database.Length == 0 || user.Length == 0)
            {
                return Fail("HO database connection details (HostName, DbName, UserName) are required.");
            }

            string connectionString = BuildTenantConnectionString(host, database, user, password);
            DbContextOptionsBuilder<OrganizationDbContext> optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            tenant = new OrganizationDbContext(optionsBuilder.Options);
            return new HeadOfficeActionResult { Success = true };
        }

        private string BuildTenantConnectionString(string host, string database, string user, string password)
        {
            string portText = _configuration["HeadOfficeTenant:Port"] ?? "3308";
            int port;
            if (!int.TryParse(portText, out port) || port <= 0)
            {
                port = 3308;
            }

            return "Host=" + host
                + ";Port=" + port.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ";Database=" + database
                + ";Username=" + user
                + ";Password=" + password;
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

        private static HeadOfficeActionResult Ok(string message, int headOfficeId)
        {
            return new HeadOfficeActionResult { Success = true, Message = message, HeadOfficeId = headOfficeId };
        }

        private static HeadOfficeActionResult Fail(string message)
        {
            return new HeadOfficeActionResult { Success = false, Message = message };
        }

        private static HeadOffice CopyWithoutSecrets(HeadOffice source)
        {
            HeadOffice row = new HeadOffice();
            ApplyWithoutSecrets(row, source);
            row.CreatedDate = source.CreatedDate;
            row.CreatedBy = source.CreatedBy;
            row.Status = source.Status;
            row.UserCreatedStatus = source.UserCreatedStatus;
            return row;
        }

        private static void ApplyWithoutSecrets(HeadOffice target, HeadOffice source)
        {
            target.HeadOfficeCode = (source.HeadOfficeCode ?? string.Empty).Trim().ToLowerInvariant();
            target.HeadOfficeName = source.HeadOfficeName;
            target.TypeId = source.TypeId;
            target.BelongsTo = source.BelongsTo;
            target.Designation = source.Designation;
            target.OrgMailId = source.OrgMailId;
            target.InchargeName = source.InchargeName;
            target.Address = source.Address;
            target.City = source.City;
            target.MobileNo = source.MobileNo;
            target.PhoneNo = source.PhoneNo;
            target.CountryId = source.CountryId;
            target.StateId = source.StateId;
            target.Pincode = source.Pincode;
            target.CountryCode = source.CountryCode;
            target.SrInchargeName = source.SrInchargeName;
            target.SrMobileNo = source.SrMobileNo;
            target.SrPhoneNo = source.SrPhoneNo;
            target.SrMailId = source.SrMailId;
            target.AccountingYearType = source.AccountingYearType;
            target.Status = source.Status;
            target.UserCreatedStatus = source.UserCreatedStatus;
            target.HostName = null;
            target.DbName = null;
            target.UserName = null;
            target.Password = null;
        }
    }
}
