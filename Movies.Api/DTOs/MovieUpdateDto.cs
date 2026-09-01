using System.ComponentModel.DataAnnotations;

namespace Movies.Api.DTOs
{
    public class MovieUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = String.Empty;
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = String.Empty;
        [Required]
        public DateTime ReleaseDate { get; set; }
        [Required]
        [StringLength(100)]
        public string Genre { get; set; } = String.Empty;
        [Required]
        [Range(0, 10)]
        public double Rating { get; set; }
        [Required]
        [Range(1, 1000)]
        public int DurationMinutes { get; set; }
    }
}