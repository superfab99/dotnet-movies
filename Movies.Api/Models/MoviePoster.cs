using Movies.Api.Models;

namespace Movies.Api.Models
{
    public class MoviePoster : BaseModel
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
    }
}