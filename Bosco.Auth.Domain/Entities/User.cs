/*  Class Name      : User.cs
 *  Purpose         : user_info row used by AuthenticateUser
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
namespace Bosco.Auth.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string EmailId { get; set; } = string.Empty;

        public string ContactNo { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public int Status { get; set; }

        public int PasswordStatus { get; set; }

        public int? UserType { get; set; }

        public string? HeadOfficeCode { get; set; }

        public string? BranchCode { get; set; }

        public string? Address { get; set; }

        public string? Notes { get; set; }

        public string? Place { get; set; }

        public string? City { get; set; }

        public string? CountryCode { get; set; }

        public int CreatedBy { get; set; }

    }
}
