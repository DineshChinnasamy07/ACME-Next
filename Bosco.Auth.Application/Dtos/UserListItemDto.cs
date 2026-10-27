using System;
using System.Collections.Generic;
using System.Text;

namespace Bosco.Auth.Application.Dtos
{
    public class UserListItemDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Status { get; set; }
        public string UserStatus { get; set; } = string.Empty;
    }
}
