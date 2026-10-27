using System;
using System.Collections.Generic;
using System.Text;

namespace Bosco.Auth.Application.Dtos
{
    public class UserSaveRequest
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}
