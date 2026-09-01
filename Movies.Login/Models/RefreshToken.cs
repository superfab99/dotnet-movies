using System.ComponentModel.DataAnnotations;

namespace Movies.Login.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOnUtc { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? RevokedOnUtc { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresOnUtc;
        public bool IsActive => RevokedOnUtc == null && !IsExpired;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}