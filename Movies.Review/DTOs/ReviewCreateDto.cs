using System.ComponentModel.DataAnnotations;

namespace Movies.Review.DTOs
{
    public class ReviewCreateDto
    {
        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = String.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public int MovieId { get; set; }

    }
}