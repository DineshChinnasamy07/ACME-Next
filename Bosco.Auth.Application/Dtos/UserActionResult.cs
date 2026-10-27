using System;
using System.Collections.Generic;
using System.Text;

namespace Bosco.Auth.Application.Dtos
{
    public class UserActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int UserId { get; set; }
    }
}
