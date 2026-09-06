using Movies.Review.Models;

namespace Movies.Review.Repositories
{
    public interface IReviewsRepository : IRepository<MovieReview>
    {
        Task<List<MovieReview>> GetByMovieIdAsync(int movieId);
    }
}
