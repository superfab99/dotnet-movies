using System.ComponentModel.DataAnnotations;

namespace Movies.Login.Models
{
    public class Roles
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
    }
}