using Microsoft.EntityFrameworkCore;
using Movies.Contracts.Movies;
using Movies.Review.Data;
using Movies.Review.Models;

namespace Movies.Review.Repositories
{
    public class MoviesRepository : Repository<Movie>, IMoviesRepository
    {
        public MoviesRepository(MoviesReviewDbContext dbContext) : base(dbContext)
        {

        }

        public async Task<Movie?> GetBySourceMovieIdAsync(int sourceMovieId)
        {
            return await _dbSet.FirstOrDefaultAsync(movie => movie.SourceMovieId == sourceMovieId);
        }

        public async Task<bool> DeleteBySourceMovieIdAsync(int sourceMovieId)
        {
            var movie = await GetBySourceMovieIdAsync(sourceMovieId);
            if (movie is null)
            {
                return false;
            }

            Delete(movie);
            return await SaveChangesAsync();
        }

        public async Task<bool> UpdateMovieBySourceMovieIdAsync(int sourceMovieId, MovieUpdated updatedMovie)
        {
            var movie = await GetBySourceMovieIdAsync(sourceMovieId);
            if (movie is null)
            {
                return false;
            }

            movie.ReleaseDate = updatedMovie.ReleaseDate;
            movie.Genre = updatedMovie.Genre;
            movie.Title = updatedMovie.Title;
            movie.ModifiedAt = DateTime.UtcNow;

            _dbSet.Update(movie);
            return await SaveChangesAsync();
        }
    }
}