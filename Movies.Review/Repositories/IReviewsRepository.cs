using Movies.Review.Models;

namespace Movies.Api.Repositories
{
    public interface IReviewsRepository : IRepository<MovieReview>
    {
        Task<List<MovieReview>> GetByMovieIdAsync(int movieId);
    }
}
