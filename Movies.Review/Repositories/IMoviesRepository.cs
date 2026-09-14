using Movies.Contracts.Movies;
using Movies.Review.Models;

namespace Movies.Review.Repositories
{
    public interface IMoviesRepository : IRepository<Movie>
    {
        Task<Movie?> GetBySourceMovieIdAsync(int sourceMovieId);
        Task<bool> DeleteBySourceMovieIdAsync(int sourceMovieId);
        Task<bool> UpdateMovieBySourceMovieIdAsync(int sourceMovieId, MovieUpdated updatedMovie);
    }
}