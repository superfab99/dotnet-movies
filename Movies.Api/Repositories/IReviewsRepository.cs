using Movies.Api.Models;

namespace Movies.Api.Repositories
{
    public interface IReviewsRepository : IRepository<Review>
    {
        Task<List<Review>> GetByMovieIdAsync(int movieId);
    }
}
