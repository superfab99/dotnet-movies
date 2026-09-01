using System.ComponentModel.DataAnnotations;

namespace Movies.Api.Models
{
    public class Review : BaseModel
    {
        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;
        [Range(1, 5)]
        public int Rating { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }
}