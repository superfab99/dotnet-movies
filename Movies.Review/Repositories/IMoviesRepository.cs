using Movies.Review.Models;

namespace Movies.Review.Repositories
{
    public interface IMoviesRepository : IRepository<Movie>
    {
        Task<Movie?> GetBySourceMovieIdAsync(int sourceMovieId);
    }
}