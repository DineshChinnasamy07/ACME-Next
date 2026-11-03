/*  Class Name      : UserSystem.cs
 *  Purpose         : AuthenticateUser chooses the portal database or one head office database
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using System.Text.RegularExpressions;
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
        private const int ResetPasswordStatus = 1;
        private static readonly Regex NewPasswordRule = new Regex(
            @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[$@$!%^()\-*#?&])[A-Za-z\d$@$!%^()\-*#?&]{8,}$",
            RegexOptions.Compiled);

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

        public async Task<UserActionResult> ChangePassword(int userId, ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            if (userId <= 0)
            {
                return new UserActionResult { Success = false, Message = "User not found." };
            }

            string currentPassword = request == null || request.CurrentPassword == null ? string.Empty : request.CurrentPassword.Trim();
            string newPassword = request == null || request.NewPassword == null ? string.Empty : request.NewPassword.Trim();
            string confirmPassword = request == null || request.ConfirmPassword == null ? string.Empty : request.ConfirmPassword.Trim();

            if (currentPassword.Length == 0 || newPassword.Length == 0 || confirmPassword.Length == 0)
            {
                return new UserActionResult { Success = false, Message = "Current password, new password, and confirm password are required." };
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                return new UserActionResult { Success = false, Message = "New password and confirm password do not match." };
            }

            if (!NewPasswordRule.IsMatch(newPassword))
            {
                return new UserActionResult { Success = false, Message = "New password must be at least 8 characters and include a letter, a number, and a symbol." };
            }

            string encryptedCurrent = _passwords.EncryptValue(currentPassword);
            bool oldOk = await _users.CheckCurrentPassword(userId, encryptedCurrent, currentPassword, cancellationToken);
            if (!oldOk)
            {
                return new UserActionResult { Success = false, Message = "Please Enter Correct Old Password" };
            }

            string encryptedNew = _passwords.EncryptValue(newPassword);
            return await _users.ResetPassword(userId, encryptedNew, ResetPasswordStatus, cancellationToken);
        }

        public async Task<UserActionResult> SetPassword(int userId, string oldPassword, string newPassword, string confirmPassword, CancellationToken cancellationToken)
        {
            if (userId <= 0)
            {
                return new UserActionResult { Success = false, Message = "User not found." };
            }

            string currentPassword = oldPassword == null ? string.Empty : oldPassword.Trim();
            string nextPassword = newPassword == null ? string.Empty : newPassword.Trim();
            string confirm = confirmPassword == null ? string.Empty : confirmPassword.Trim();
            if (currentPassword.Length == 0 || nextPassword.Length == 0 || confirm.Length == 0)
            {
                return new UserActionResult { Success = false, Message = "Old password, new password, and confirm password are required." };
            }

            if (!string.Equals(nextPassword, confirm, StringComparison.Ordinal))
            {
                return new UserActionResult { Success = false, Message = "New password and confirm password do not match." };
            }

            if (!NewPasswordRule.IsMatch(nextPassword))
            {
                return new UserActionResult { Success = false, Message = "New password must be at least 8 characters and include a letter, a number, and a symbol." };
            }

            string encryptedCurrent = _passwords.EncryptValue(currentPassword);
            bool oldOk = await _users.CheckCurrentPassword(userId, encryptedCurrent, currentPassword, cancellationToken);
            if (!oldOk)
            {
                return new UserActionResult { Success = false, Message = "Please Enter Correct Old Password" };
            }

            string encryptedNew = _passwords.EncryptValue(nextPassword);
            UserActionResult result = await _users.ResetPassword(userId, encryptedNew, ResetPasswordStatus, cancellationToken);
            if (result.Success)
            {
                result.Message = "Password has been changed successfully.";
            }

            return result;
        }
    }
}
