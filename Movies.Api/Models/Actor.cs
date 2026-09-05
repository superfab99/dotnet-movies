using System.ComponentModel.DataAnnotations;
using Movies.Api.Enums;

namespace Movies.Api.Models
{
    public class Actor : BaseModel
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public Gender Gender { get; set; }
        public string? ProfileImageUrl { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}