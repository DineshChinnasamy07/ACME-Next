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
        //Fetch user  
        Task<IReadOnlyList<UserListItemDto>> FetchUserDetail(int createdBy, CancellationToken cancellationToken);

        Task<UserSaveRequest?> FetchUserDetailsById(int userId, CancellationToken cancellationToken);

        Task<UserActionResult> SaveUser(UserSaveRequest request, int createdBy, string encryptedPassword, CancellationToken cancellationToken);

        Task<UserActionResult> DeleteUser(int userId, CancellationToken cancellationToken);

        Task<UserActionResult> UpdateUserStatus(int userId, int status, CancellationToken cancellationToken);

    }
}
