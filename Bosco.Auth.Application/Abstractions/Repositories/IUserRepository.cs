/*  Class Name      : IUserRepository.cs
 *  Purpose         : AuthenticateUser lookup on the portal or one head office database
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Application.DTOs;
using Bosco.Auth.Domain.Entities;
using Bosco.Contracts.Data;

namespace Bosco.Auth.Application.Abstractions.Repositories
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<UserDto>> AuthenticateUser(string userName, string encryptedPassword, string passWord, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken);

        // office == null runs on the portal database; otherwise on that head office database.
        Task<IReadOnlyList<UserListItemDto>> FetchUserDetail(int createdBy, HeadOffice? office, CancellationToken cancellationToken);

        // Role list for the user form dropdown (same database rule as the user list).
        Task<IReadOnlyList<RoleListItemDto>> FetchUserRoles(HeadOffice? office, CancellationToken cancellationToken);

        Task<UserSaveRequest?> FetchUserDetailsById(int userId, HeadOffice? office, CancellationToken cancellationToken);

        Task<UserActionResult> SaveUser(UserSaveRequest request, int createdBy, string encryptedPassword, HeadOffice? office, CancellationToken cancellationToken);

        Task<UserActionResult> DeleteUser(int userId, HeadOffice? office, CancellationToken cancellationToken);

        Task<UserActionResult> UpdateUserStatus(int userId, int status, HeadOffice? office, CancellationToken cancellationToken);

        Task<bool> CheckCurrentPassword(int userId, string encryptedPassword, string passWord, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken);

        Task<UserActionResult> ResetPassword(int userId, string encryptedPassword, int passwordStatus, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken);
    }
}
