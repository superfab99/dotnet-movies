using System.ComponentModel.DataAnnotations;

namespace Movies.Login.Models
{
    public class UserRoles
    {
        [Required]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [Required]
        public int RoleId { get; set; }

        public Roles Role { get; set; } = null!;
    }
}