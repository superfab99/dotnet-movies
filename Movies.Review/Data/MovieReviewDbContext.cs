using Microsoft.EntityFrameworkCore;
using Movies.Review.Models;

namespace Movies.Review.Data
{
    public class MoviesReviewDbContext : DbContext
    {
        public MoviesReviewDbContext(DbContextOptions<MoviesReviewDbContext> options) : base(options)
        {

        }

        public DbSet<MovieReview> MovieReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(MoviesReviewDbContext).Assembly);
        }
    }
}