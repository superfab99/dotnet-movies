using System.ComponentModel.DataAnnotations;
using Movies.Api.Enums;

namespace Movies.Api.DTOs
{
    public class ActorCreateDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public Gender Gender { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}