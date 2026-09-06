namespace Movies.Review.Models
{
    public class Movie : BaseModel
    {
        public int SourceMovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
    }
}