/*  Class Name      : UserRepository.cs
 *  Purpose         : LINQ lookup for AuthenticateUser
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Application.Abstractions.Repositories;
using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Application.DTOs;
using Bosco.Auth.Domain.Entities;
using Bosco.Auth.Infrastructure.Persistence;
using Bosco.Contracts.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bosco.Auth.Infrastructure.Repositories
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

        public async Task<IReadOnlyList<UserListItemDto>> FetchUserDetail(int createdBy, CancellationToken cancellationToken)
        {
            return await (
                from user in _portal.Users.AsNoTracking()
                join role in _portal.Roles.AsNoTracking() on user.RoleId equals role.UserRoleId into roles
                from role in roles.DefaultIfEmpty()
                where user.CreatedBy == createdBy
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
                    Status = user.Status,
                    UserStatus = user.Status == 1 ? "Active" : "Inactive"
                }).ToListAsync(cancellationToken);
        }

        public async Task<UserSaveRequest?> FetchUserDetailsById(int userId,CancellationToken cancellationToken) 
        {
            return await _portal.Users.AsNoTracking()
                .Where(user => user.UserId == userId)
                .Select(user => new UserSaveRequest
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName ?? string.Empty,
                    UserName = user.UserName,
                    Address = user.Address ?? string.Empty,
                    MobileNo = user.ContactNo,
                    Email = user.EmailId,
                    Notes = user.Notes ?? string.Empty,
                    Place = user.Place ?? string.Empty,
                    City = user.City ?? string.Empty,
                    CountryCode = user.CountryCode ?? string.Empty,
                    RoleId = user.RoleId
                }).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<UserActionResult> SaveUser(UserSaveRequest request,int createdBy,string encryptedPassword,CancellationToken cancellationToken) 
        {
            bool duplicate = await _portal.Users.AnyAsync(user => user.UserName == request.UserName && user.UserId != request.UserId, cancellationToken);
            if (duplicate) 
            {
                return new UserActionResult
                {
                    Success = false,
                    Message = "User name already exists."
                };
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
                    RoleId = request.RoleId,
                    Notes = request.Notes,
                    Status = 1,
                    Place = request.Place,
                    City = request.City,
                    PasswordStatus = 1,
                    UserType = 0,
                    HeadOfficeCode = null,
                    BranchCode = null,
                    CountryCode = request.CountryCode,
                    CreatedBy = createdBy
                };
                _portal.Users.Add(row);
                await _portal.SaveChangesAsync(cancellationToken);
                return new UserActionResult
                {
                    Success = true,
                    Message = "User Saved.",
                    UserId = row.UserId
                };
            }
            User? existing = await _portal.Users.FirstOrDefaultAsync(user => user.UserId == request.UserId, cancellationToken);
            if (existing == null)
            {
                return new UserActionResult { Success = false, Message = "User not found." };
            }
            existing.FirstName = request.FirstName;
            existing.LastName = request.LastName;
            existing.UserName = request.UserName;
            existing.Address = request.Address;
            existing.ContactNo = request.MobileNo;
            existing.EmailId = request.Email;
            existing.RoleId = existing.UserId == 1 ? existing.RoleId : request.RoleId;
            existing.Notes = request.Notes;
            existing.Place = request.Place;
            existing.City = request.City;
            existing.CountryCode = request.CountryCode;
            existing.CreatedBy = createdBy;
            await _portal.SaveChangesAsync(cancellationToken);
            return new UserActionResult 
            { 
                Success = true, 
                Message = "User saved.",
                UserId = existing.UserId 
            };
        }

        public async Task<UserActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
        {
            if (userId == 1)
            {
                return new UserActionResult 
                { 
                    Success = false, 
                    Message = "Cannot Delete Admin User"
                };
            }
            User? existing = await _portal.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
            if (existing == null)
            {
                return new UserActionResult { Success = false, Message = "User not found." };
            }
            if (existing.Status == 1)
            {
                return new UserActionResult { Success = false, Message = "Cannot delete an active user." };
            }
            _portal.Users.Remove(existing);
            await _portal.SaveChangesAsync(cancellationToken);
            return new UserActionResult 
            { 
                Success = true, 
                Message = "User deleted.", 
                UserId = userId 
            };
        }


        public async Task<UserActionResult> UpdateUserStatus(int userId,int status, CancellationToken cancellationToken) 
        {
            User? existing = await _portal.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
            if (existing == null) 
            {
                return new UserActionResult
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            if (userId == 1 && status == 0)
            {
                return new UserActionResult
                {
                    Success = false,
                    Message = "Cannot deactivate the Admin user."
                };
            }
            existing.Status = status;
            await _portal.SaveChangesAsync(cancellationToken);
            string stateName = status == 1 ? "Activated" : "Deactivated";
            return new UserActionResult
            {
                Success = true,
                Message = "User " + stateName,
                UserId = userId
            };
        }

    }
}
