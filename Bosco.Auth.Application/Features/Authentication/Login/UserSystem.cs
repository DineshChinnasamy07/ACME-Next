/*  Class Name      : UserSystem.cs
 *  Purpose         : AuthenticateUser chooses the portal database or one head office database
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Application.Abstractions.Repositories;
using Bosco.Auth.Application.Abstractions.Services;
using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Application.DTOs;
using Bosco.Auth.Domain.Entities;
using Bosco.Contracts.Data;

namespace Bosco.Auth.Application.Features.Authentication.Login
{
    public class UserSystem
    {
        public const string PortalCode = "portal";
        private const int AutomaticPassword = 0;

        private readonly IHeadOfficeRepository _headOffices;
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwords;
        private readonly IJwtTokenService _tokens;

        public UserSystem(IHeadOfficeRepository headOffices, IUserRepository users, IPasswordHasher passwords, IJwtTokenService tokens)
        {
            _headOffices = headOffices;
            _users = users;
            _passwords = passwords;
            _tokens = tokens;
        }

        public async Task<AuthenticationOutcome> AuthenticateUser(string headOfficeCode, string userName, string passWord, CancellationToken cancellationToken)
        {
            headOfficeCode = headOfficeCode == null ? string.Empty : headOfficeCode.Trim();
            userName = userName == null ? string.Empty : userName.Trim();
            passWord = passWord == null ? string.Empty : passWord.Trim();

            if (headOfficeCode.Length == 0 || userName.Length == 0 || passWord.Length == 0)
            {
                return AuthenticationOutcome.Fail(AuthenticationOutcome.Validation, "Head office code, user name, and password are required.");
            }

            if (string.Equals(headOfficeCode, PortalCode, StringComparison.OrdinalIgnoreCase))
            {
                return await AuthenticateUser(userName, passWord, DataBaseType.Portal, null, cancellationToken);
            }

            HeadOffice? office = await _headOffices.FetchDatabase(headOfficeCode, cancellationToken);
            if (office == null || string.IsNullOrWhiteSpace(office.HostName) || string.IsNullOrWhiteSpace(office.DbName))
            {
                return AuthenticationOutcome.Fail(AuthenticationOutcome.InvalidHeadOffice, "Invalid Head Office Code");
            }

            return await AuthenticateUser(userName, passWord, DataBaseType.HeadOffice, office, cancellationToken);
        }

        public async Task<AuthenticationOutcome> AuthenticateUser(string userName, string passWord, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken)
        {
            string encrypted = _passwords.EncryptValue(passWord);
            IReadOnlyList<UserDto> matches;
            try
            {
                matches = await _users.AuthenticateUser(userName, encrypted, passWord, connectTo, office, cancellationToken);
            }
            catch (Exception)
            {
                return AuthenticationOutcome.Fail(AuthenticationOutcome.Unavailable, "Server is under the maintenance. sorry for the inconvenience.");
            }

            if (matches.Count != 1)
            {
                return AuthenticationOutcome.Fail(AuthenticationOutcome.InvalidUser, "Invalid User Name and Password");
            }

            UserDto user = matches[0];
            user.PasswordChangeRequired = user.PasswordStatus == AutomaticPassword;

            LoginResponseDto response = new LoginResponseDto
            {
                AccessToken = _tokens.CreateAccessToken(user),
                TokenType = "Bearer",
                ExpiresInMinutes = _tokens.ExpiryMinutes,
                PasswordChangeRequired = user.PasswordChangeRequired,
                User = user
            };
            return AuthenticationOutcome.Ok(response);
        }

        public LogoutResult SetLogout()
        {
            return new LogoutResult
            {
                Success = true,
                TokenRevoked = false,
                Message = "Signed out. Discard the bearer token. It remains valid until it expires."
            };
        }

        public Task<IReadOnlyList<UserListItemDto>> FetchUserDetail(int createdBy, CancellationToken cancellationToken)
        {
            return _users.FetchUserDetail(createdBy, cancellationToken);
        }

        public Task<UserSaveRequest?> FetchUserDetailsById(int userId, CancellationToken cancellationToken)
        {
            return _users.FetchUserDetailsById(userId, cancellationToken);
        }

        public Task<UserActionResult> SaveUser(UserSaveRequest request, int createdBy, CancellationToken cancellationToken)
        {
            request.FirstName = request.FirstName == null ? string.Empty : request.FirstName.Trim();
            request.LastName = request.LastName == null ? string.Empty : request.LastName.Trim();
            request.UserName = request.UserName == null ? string.Empty : request.UserName.Trim();
            request.Password = request.Password == null ? string.Empty : request.Password.Trim();
            request.MobileNo = request.MobileNo == null ? string.Empty : request.MobileNo.Trim();
            request.Email = request.Email == null ? string.Empty : request.Email.Trim().ToLower();
            request.Address = request.Address == null ? string.Empty : request.Address.Trim();
            request.Notes = request.Notes == null ? string.Empty : request.Notes.Trim();
            request.Place = request.Place == null ? string.Empty : request.Place.Trim();
            request.City = request.City == null ? string.Empty : request.City.Trim();
            request.CountryCode = request.CountryCode == null ? string.Empty : request.CountryCode.Trim();

            if (request.FirstName.Length == 0 || request.UserName.Length == 0 || request.MobileNo.Length == 0 || request.Email.Length == 0 || request.RoleId <= 0)
            {
                return Task.FromResult(new UserActionResult { Success = false, Message = "Name, user name, mobile number, email, and role are required." });
            }

            if (request.UserId == 0 && request.Password.Length == 0)
            {
                return Task.FromResult(new UserActionResult { Success = false, Message = "Password is required." });
            }

            string encryptedPassword = request.UserId == 0 ? _passwords.EncryptValue(request.Password) : string.Empty;
            return _users.SaveUser(request, createdBy, encryptedPassword, cancellationToken);
        }

        public Task<UserActionResult> UpdateUserStatus(int userId, int status, CancellationToken cancellationToken)
        {
            if (status != 0 && status != 1)
            {
                return Task.FromResult(new UserActionResult { Success = false, Message = "Status must be 0 or 1." });
            }
            return _users.UpdateUserStatus(userId, status, cancellationToken);
        }

        public Task<UserActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
        {
            return _users.DeleteUser(userId, cancellationToken);
        }
    }
}
