using Movies.Review.DTOs;

namespace Movies.Review.Services
{
    public interface IReviewsService
    {
        Task<ReviewDto> CreateReviewAsync(ReviewCreateDto reviewDto);
        Task<bool> DeleteReviewAsync(int id);
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<List<ReviewDto>> GetReviewsByMovieIdAsync(int movieId);
    }
}