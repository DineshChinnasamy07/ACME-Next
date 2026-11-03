/*  Class Name      : TenantUser.cs
 *  Purpose         : HO tenant user_info row for activate-admin dual-write
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
namespace Bosco.Organization.Domain.Entities
{
    public class TenantUser
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

        public int CreatedBy { get; set; }
    }
}
