namespace Movies.Api.DTOs
{
    public class MoviesDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public string Genre { get; set; } = String.Empty;
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public MoviePosterDto? MoviePoster { get; set; }
        public List<MovieActorDto> MovieActors { get; set; } = new();
    }
}