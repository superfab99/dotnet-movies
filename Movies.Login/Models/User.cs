using System.ComponentModel.DataAnnotations;

namespace Movies.Login.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string PasswordHash { get; set; }

        [EmailAddress]
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}