namespace Movies.Api.DTOs
{
    public class MoviePosterUploadDto
    {
        public IFormFile Image { get; set; } = null!;
        public string AltText { get; set; } = string.Empty;
    }
}