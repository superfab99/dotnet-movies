namespace Movies.Api.DTOs
{
    public class MoviePosterDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string ImageUrl { get; set; } = String.Empty;
        public string AltText { get; set; } = string.Empty;
    }
}