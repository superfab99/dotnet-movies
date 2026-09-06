using Microsoft.EntityFrameworkCore;
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
    }
}