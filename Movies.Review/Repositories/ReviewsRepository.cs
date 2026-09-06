using Microsoft.EntityFrameworkCore;
using Movies.Review.Data;
using Movies.Review.Models;

namespace Movies.Review.Repositories
{
    public class ReviewsRepository : Repository<MovieReview>, IReviewsRepository
    {
        private readonly ILogger<ReviewsRepository> _logger;

        public ReviewsRepository(MoviesReviewDbContext dbContext, ILogger<ReviewsRepository> logger) : base(dbContext)
        {
            _logger = logger;
        }

        public async Task<List<MovieReview>> GetByMovieIdAsync(int movieId)
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
