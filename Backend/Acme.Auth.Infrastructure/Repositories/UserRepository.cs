/*  Class Name      : UserRepository.cs
 *  Purpose         : LINQ lookup for AuthenticateUser
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Acme.Auth.Application.Abstractions.Repositories;
using Acme.Auth.Application.Dtos;
using Acme.Auth.Application.DTOs;
using Acme.Auth.Domain.Entities;
using Acme.Auth.Infrastructure.Persistence;
using Acme.Contracts.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
// Aliases instead of "using Acme.Common;" because Acme.Common also has an enum named User,
// which clashes with Acme.Auth.Domain.Entities.User.
using MessageCatalog = Acme.Common.MessageCatalog;
using MessageRender = Acme.Common.MessageRender;

namespace Acme.Auth.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private const int ActiveStatus = 1;
        private readonly AuthDbContext _portal;
        private readonly IConfiguration _configuration;

        public UserRepository(AuthDbContext portal, IConfiguration configuration)
        {
            _portal = portal;
            _configuration = configuration;
        }

        #region Helper Methode
        // office == null runs on the portal context (scoped by DI, never disposed here).
        // Otherwise runs on a short-lived context for that head office database.
        private async Task<T> Run<T>(HeadOffice? office, Func<AuthDbContext, Task<T>> action)
        {
            if (office == null)
            {
                await EnsureGenderColumn(_portal);
                return await action(_portal);
            }

            using (AuthDbContext headOffice = CreateHeadOfficeContext(office))
            {
                await EnsureGenderColumn(headOffice);
                return await action(headOffice);
            }
        }

        // user_info.GENDER was added on 13-11-2026. Databases created before that do not have it, so the first
        // user query on each database adds it (IF NOT EXISTS keeps this safe). The check runs once per database
        // for the life of the API process. The database login must be allowed to ALTER user_info.
        private static readonly HashSet<string> GenderChecked = new HashSet<string>();
        private static readonly object GenderLock = new object();

        private static async Task EnsureGenderColumn(AuthDbContext context)
        {
            string key = context.Database.GetConnectionString() ?? string.Empty;
            lock (GenderLock)
            {
                if (GenderChecked.Contains(key))
                {
                    return;
                }
            }

            await context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE public.user_info ADD COLUMN IF NOT EXISTS \"GENDER\" character varying(1) DEFAULT 'M'::character varying NOT NULL");

            lock (GenderLock)
            {
                GenderChecked.Add(key);
            }
        }

        private static UserActionResult Fail(string key)
        {
            return new UserActionResult { Success = false, Message = MessageRender.GetMessage(key) };
        }

        private static UserActionResult Ok(string key, int userId)
        {
            return new UserActionResult { Success = true, Message = MessageRender.GetMessage(key), UserId = userId };
        }

        // Portal BOLA guard: list is scoped by CreatedBy, so by-id actions must match. Head office DB is already tenant-scoped.
        private static bool CanAccessPortalUser(User user, int actorUserId, HeadOffice? office)
        {
            if (office != null)
            {
                return true;
            }

            if (actorUserId <= 0)
            {
                return false;
            }

            return user.CreatedBy == actorUserId || user.UserId == actorUserId;
        }
        #endregion


        public async Task<IReadOnlyList<UserDto>> AuthenticateUser(string userName, string encryptedPassword, string passWord, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken)
        {
            if (connectTo == DataBaseType.HeadOffice)
            {
                if (office == null)
                {
                    return new List<UserDto>();
                }

                string connectionString = HeadOfficeConnection.Build(office, _configuration);
                DbContextOptions<AuthDbContext> options = new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(connectionString).Options;
                using (AuthDbContext headOffice = new AuthDbContext(options))
                {
                    return await Find(headOffice, userName, encryptedPassword, passWord, cancellationToken);
                }
            }

            return await Find(_portal, userName, encryptedPassword, passWord, cancellationToken);
        }

        private static async Task<IReadOnlyList<UserDto>> Find(AuthDbContext context, string userName, string encryptedPassword, string passWord, CancellationToken cancellationToken)
        {
            return await (
                from user in context.Users.AsNoTracking()
                join role in context.Roles.AsNoTracking() on user.RoleId equals role.UserRoleId into roles
                from role in roles.DefaultIfEmpty()
                join branch in context.BranchOffices.AsNoTracking() on user.BranchCode equals branch.BranchOfficeCode into branches
                from branch in branches.DefaultIfEmpty()
                where user.UserName == userName
                    && user.Status == ActiveStatus
                    && (user.Password == encryptedPassword || user.Password == passWord)
                select new UserDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName ?? string.Empty,
                    RoleId = user.RoleId,
                    UserRole = role == null ? string.Empty : role.UserRole,
                    UserType = user.UserType,
                    HeadOfficeCode = user.HeadOfficeCode ?? string.Empty,
                    BranchCode = user.BranchCode ?? string.Empty,
                    BranchOfficeName = branch == null ? string.Empty : branch.BranchOfficeName,
                    PasswordStatus = user.PasswordStatus
                }).ToListAsync(cancellationToken);
        }

        public Task<IReadOnlyList<UserListItemDto>> FetchUserDetail(int createdBy, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run<IReadOnlyList<UserListItemDto>>(office, async context =>
            {
                IQueryable<User> users = context.Users.AsNoTracking();
                if (office == null)
                {
                    // Portal: users created by the signed-in portal user.
                    users = users.Where(user => user.CreatedBy == createdBy);
                }
                else
                {
                    // Head office database: head office users only (branch users have a BRANCH_CODE).
                    users = users.Where(user => user.BranchCode == null || user.BranchCode == string.Empty);
                }

                return await (
                    from user in users
                    join role in context.Roles.AsNoTracking() on user.RoleId equals role.UserRoleId into roles
                    from role in roles.DefaultIfEmpty()
                    orderby user.UserName
                    select new UserListItemDto
                    {
                        UserId = user.UserId,
                        Name = ((user.FirstName ?? string.Empty) + " " + (user.LastName ?? string.Empty)).Trim(),
                        UserName = user.UserName,
                        RoleId = user.RoleId,
                        UserRole = role == null ? string.Empty : role.UserRole,
                        ContactNo = user.ContactNo,
                        Email = user.EmailId,
                        Gender = user.Gender,
                        Status = user.Status,
                        UserStatus = user.Status == 1 ? "Active" : "Inactive"
                    }).ToListAsync(cancellationToken);
            });
        }

        public Task<IReadOnlyList<RoleListItemDto>> FetchUserRoles(HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run<IReadOnlyList<RoleListItemDto>>(office, async context =>
            {
                return await context.Roles
                    .AsNoTracking()
                    .Where(role => role.UserRole != null && role.Accessibility == 0)
                    .OrderBy(role => role.UserRole)
                    .Select(role => new RoleListItemDto
                    {
                        UserRoleId = role.UserRoleId,
                        Name = role.UserRole
                    })
                    .ToListAsync(cancellationToken);
            });
        }

        public Task<UserSaveRequest?> FetchUserDetailsById(int userId, int actorUserId, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run<UserSaveRequest?>(office, async context =>
            {
                User? row = await context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
                if (row == null || !CanAccessPortalUser(row, actorUserId, office))
                {
                    return null;
                }

                return new UserSaveRequest
                {
                    UserId = row.UserId,
                    FirstName = row.FirstName,
                    LastName = row.LastName ?? string.Empty,
                    UserName = row.UserName,
                    Address = row.Address ?? string.Empty,
                    MobileNo = row.ContactNo,
                    Email = row.EmailId,
                    Gender = row.Gender,
                    Notes = row.Notes ?? string.Empty,
                    Place = row.Place ?? string.Empty,
                    City = row.City ?? string.Empty,
                    CountryCode = row.CountryCode ?? string.Empty,
                    RoleId = row.RoleId
                };
            });
        }

        public Task<UserActionResult> SaveUser(UserSaveRequest request, int createdBy, string encryptedPassword, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run(office, async context =>
            {
                bool duplicate = await context.Users.AnyAsync(user => user.UserName == request.UserName && user.UserId != request.UserId, cancellationToken);
                if (duplicate)
                {
                    return Fail(MessageCatalog.User.NameExists);
                }

                if (request.UserId == 0)
                {
                    User row = new User
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        UserName = request.UserName,
                        Password = encryptedPassword,
                        Address = request.Address,
                        ContactNo = request.MobileNo,
                        EmailId = request.Email,
                        Gender = request.Gender,
                        RoleId = request.RoleId,
                        Notes = request.Notes,
                        Status = 1,
                        Place = request.Place,
                        City = request.City,
                        PasswordStatus = office == null ? 1 : 0, // head office users change the password at first login
                        UserType = 0,
                        HeadOfficeCode = office == null ? null : office.HeadOfficeCode,
                        BranchCode = null,
                        CountryCode = request.CountryCode,
                        CreatedBy = createdBy
                    };
                    context.Users.Add(row);
                    await context.SaveChangesAsync(cancellationToken);
                    return Ok(MessageCatalog.User.Saved, row.UserId);
                }

                User? existing = await context.Users.FirstOrDefaultAsync(user => user.UserId == request.UserId, cancellationToken);
                if (existing == null || !CanAccessPortalUser(existing, createdBy, office))
                {
                    return Fail(MessageCatalog.User.NotFound);
                }

                existing.FirstName = request.FirstName;
                existing.LastName = request.LastName;
                existing.UserName = request.UserName;
                existing.Address = request.Address;
                existing.ContactNo = request.MobileNo;
                existing.EmailId = request.Email;
                existing.Gender = request.Gender;
                existing.RoleId = existing.UserId == 1 ? existing.RoleId : request.RoleId;
                existing.Notes = request.Notes;
                existing.Place = request.Place;
                existing.City = request.City;
                existing.CountryCode = request.CountryCode;
                await context.SaveChangesAsync(cancellationToken);
                return Ok(MessageCatalog.User.Saved, existing.UserId);
            });
        }

        public Task<UserActionResult> DeleteUser(int userId, int actorUserId, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run(office, async context =>
            {
                if (userId == 1)
                {
                    return Fail(MessageCatalog.User.CannotDeleteAdmin);
                }

                User? existing = await context.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
                if (existing == null || !CanAccessPortalUser(existing, actorUserId, office))
                {
                    return Fail(MessageCatalog.User.NotFound);
                }

                if (existing.Status == 1)
                {
                    return Fail(MessageCatalog.User.CannotDeleteActive);
                }

                context.Users.Remove(existing);
                await context.SaveChangesAsync(cancellationToken);
                return Ok(MessageCatalog.User.Deleted, userId);
            });
        }

        public Task<UserActionResult> UpdateUserStatus(int userId, int status, int actorUserId, HeadOffice? office, CancellationToken cancellationToken)
        {
            return Run(office, async context =>
            {
                User? existing = await context.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
                if (existing == null || !CanAccessPortalUser(existing, actorUserId, office))
                {
                    return Fail(MessageCatalog.User.NotFound);
                }

                if (userId == 1 && status == 0)
                {
                    return Fail(MessageCatalog.User.CannotDeactivateAdmin);
                }

                existing.Status = status;
                await context.SaveChangesAsync(cancellationToken);
                return Ok(status == 1 ? MessageCatalog.User.Activated : MessageCatalog.User.Deactivated, userId);
            });
        }

        public async Task<bool> CheckCurrentPassword(int userId, string encryptedPassword, string passWord, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken)
        {
            if (connectTo == DataBaseType.HeadOffice)
            {
                if (office == null)
                {
                    return false;
                }

                using (AuthDbContext headOffice = CreateHeadOfficeContext(office))
                {
                    return await HasPassword(headOffice, userId, encryptedPassword, passWord, cancellationToken);
                }
            }

            return await HasPassword(_portal, userId, encryptedPassword, passWord, cancellationToken);
        }

        public async Task<UserActionResult> ResetPassword(int userId, int actorUserId, string encryptedPassword, int passwordStatus, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken)
        {
            if (connectTo == DataBaseType.HeadOffice)
            {
                if (office == null)
                {
                    return new UserActionResult { Success = false, Message = "Invalid Head Office Code" };
                }

                using (AuthDbContext headOffice = CreateHeadOfficeContext(office))
                {
                    return await SavePassword(headOffice, userId, actorUserId, office, encryptedPassword, passwordStatus, cancellationToken);
                }
            }

            return await SavePassword(_portal, userId, actorUserId, null, encryptedPassword, passwordStatus, cancellationToken);
        }

        private AuthDbContext CreateHeadOfficeContext(HeadOffice office)
        {
            string connectionString = HeadOfficeConnection.Build(office, _configuration);
            DbContextOptions<AuthDbContext> options = new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(connectionString).Options;
            return new AuthDbContext(options);
        }

        private static async Task<bool> HasPassword(AuthDbContext context, int userId, string encryptedPassword, string passWord, CancellationToken cancellationToken)
        {
            return await context.Users.AsNoTracking().AnyAsync(
                user => user.UserId == userId
                    && (user.Password == encryptedPassword || user.Password == passWord),
                cancellationToken);
        }

        private static async Task<UserActionResult> SavePassword(AuthDbContext context, int userId, int actorUserId, HeadOffice? office, string encryptedPassword, int passwordStatus, CancellationToken cancellationToken)
        {
            await EnsureGenderColumn(context); // this loads the whole User row, which includes GENDER
            User? existing = await context.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
            if (existing == null || !CanAccessPortalUser(existing, actorUserId, office))
            {
                return new UserActionResult { Success = false, Message = MessageRender.GetMessage(MessageCatalog.User.NotFound) };
            }

            existing.Password = encryptedPassword;
            existing.PasswordStatus = passwordStatus;
            await context.SaveChangesAsync(cancellationToken);
            return new UserActionResult
            {
                Success = true,
                Message = "Your password has been changed successfully.",
                UserId = userId
            };
        }

    }
}
