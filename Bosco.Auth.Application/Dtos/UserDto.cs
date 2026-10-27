/*  Class Name      : UserDto.cs
 *  Purpose         : Signed-in user identity without the password
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
namespace Bosco.Auth.Application.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string UserRole { get; set; } = string.Empty;

        public int? UserType { get; set; }

        public string HeadOfficeCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        public string BranchOfficeName { get; set; } = string.Empty;

        public int PasswordStatus { get; set; }

        public bool PasswordChangeRequired { get; set; }
    }
}
