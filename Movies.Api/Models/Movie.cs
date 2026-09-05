using System.ComponentModel.DataAnnotations;

namespace Movies.Api.Models
{
    public class Movie : BaseModel
    {
        [Required]
        public string Title { get; set; } = String.Empty;
        [Required]
        public string Description { get; set; } = String.Empty;
        [Required]
        public DateTime ReleaseDate { get; set; }
        [Required]
        public string Genre { get; set; } = String.Empty;
        [Required]
        [Range(0, 10)]
        public double Rating { get; set; }
        [Required]
        [Range(1, 1000)]
        public int DurationMinutes { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public MoviePoster? MoviePoster { get; set; }

    }
}