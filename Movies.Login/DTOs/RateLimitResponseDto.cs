namespace Movies.Login.DTOs
{
    public class RateLimitResponseDto
    {
        public string Error { get; set; } = "TooManyRequests";
        public string Message { get; set; } = "Too many requests. Please try again later.";
        public int RetryAfterSeconds { get; set; }
    }
}