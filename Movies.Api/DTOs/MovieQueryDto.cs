namespace Movies.Api.DTOs
{
    public class MovieQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public string SortBy { get; set; } = "id";
        public bool Descending { get; set; }
    }
}
