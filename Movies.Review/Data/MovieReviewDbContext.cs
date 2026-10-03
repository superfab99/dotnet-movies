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
        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>()
            .HasIndex(movie => movie.SourceMovieId)
            .IsUnique();

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(MoviesReviewDbContext).Assembly);
        }
    }
}