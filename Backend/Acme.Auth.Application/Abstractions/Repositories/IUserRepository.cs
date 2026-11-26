/*  Class Name      : IUserRepository.cs
 *  Purpose         : AuthenticateUser lookup on the portal or one head office database
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Acme.Auth.Application.Dtos;
using Acme.Auth.Application.DTOs;
using Acme.Auth.Domain.Entities;
using Acme.Contracts.Data;

namespace Acme.Auth.Application.Abstractions.Repositories
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<UserDto>> AuthenticateUser(string userName, string encryptedPassword, string passWord, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken);

        // office == null runs on the portal database; otherwise on that head office database.
        Task<IReadOnlyList<UserListItemDto>> FetchUserDetail(int createdBy, HeadOffice? office, CancellationToken cancellationToken);

        // Role list for the user form dropdown (same database rule as the user list).
        Task<IReadOnlyList<RoleListItemDto>> FetchUserRoles(HeadOffice? office, CancellationToken cancellationToken);

        // actorUserId: signed-in user. On the portal database, by-id access is limited to rows that user created.
        Task<UserSaveRequest?> FetchUserDetailsById(int userId, int actorUserId, HeadOffice? office, CancellationToken cancellationToken);

        Task<UserActionResult> SaveUser(UserSaveRequest request, int createdBy, string encryptedPassword, HeadOffice? office, CancellationToken cancellationToken);

        Task<UserActionResult> DeleteUser(int userId, int actorUserId, HeadOffice? office, CancellationToken cancellationToken);

        Task<UserActionResult> UpdateUserStatus(int userId, int status, int actorUserId, HeadOffice? office, CancellationToken cancellationToken);

        Task<bool> CheckCurrentPassword(int userId, string encryptedPassword, string passWord, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken);

        // actorUserId: for portal admin reset, the row must be created by the actor (self-service may pass the same id as userId).
        Task<UserActionResult> ResetPassword(int userId, int actorUserId, string encryptedPassword, int passwordStatus, DataBaseType connectTo, HeadOffice? office, CancellationToken cancellationToken);
    }
}
