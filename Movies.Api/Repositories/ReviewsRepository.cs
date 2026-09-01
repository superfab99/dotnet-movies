using Microsoft.EntityFrameworkCore;
using Movies.Api.Data;
using Movies.Api.Models;

namespace Movies.Api.Repositories
{
    public class ReviewsRepository : Repository<Review>, IReviewsRepository
    {
        private readonly ILogger<ReviewsRepository> _logger;

        public ReviewsRepository(MoviesApiDbContext dbContext, ILogger<ReviewsRepository> logger) : base(dbContext)
        {
            _logger = logger;
        }

        public async Task<List<Review>> GetByMovieIdAsync(int movieId)
        {
            var reviews = await _dbSet
                .AsNoTracking()
                .Where(review => review.MovieId == movieId)
                .OrderBy(review => review.Id)
                .ToListAsync();

            _logger.LogInformation("Retrieved {ReviewCount} reviews for movie {MovieId}", reviews.Count, movieId);
            return reviews;
        }
    }
}
