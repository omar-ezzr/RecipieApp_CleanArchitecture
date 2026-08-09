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
        public required string DisplayName { get; set; }
        public string Email {get; set;} = default!;
        public string PasswordHash {get; set;} = default!;

        public string Role { get; set; } = AppRoles.User;
        public bool IsActive { get; set; } = true;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CountryCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public ICollection<Recipie> Recipes { get; set; } = [];
        public ICollection<UserFollow> Following { get; set; } = [];
        public ICollection<UserFollow> Followers { get; set; } = [];
    }
}
