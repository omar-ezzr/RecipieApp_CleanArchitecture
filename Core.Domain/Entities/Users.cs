using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Constants;

namespace Core.Domain.Entities
{
    public class Users
    {
        public Guid Id {get; set;}
        public string Email {get; set;} = default!;
        public string PasswordHash {get; set;} = default!;

        public string Role { get; set; } = AppRoles.User;
        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
