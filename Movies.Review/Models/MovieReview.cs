using System.ComponentModel.DataAnnotations;

namespace Movies.Review.Models
{
    public class MovieReview : BaseModel
    {
        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;
        [Range(1, 5)]
        public int Rating { get; set; }
        public int MovieId { get; set; }
    }
}