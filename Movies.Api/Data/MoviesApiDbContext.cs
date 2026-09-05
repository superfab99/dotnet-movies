using Microsoft.EntityFrameworkCore;
using Movies.Api.Models;

namespace Movies.Api.Data
{
    public class MoviesApiDbContext : DbContext
    {
        public MoviesApiDbContext(DbContextOptions<MoviesApiDbContext> options) : base(options)
        {

        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(MoviesApiDbContext).Assembly);
        }
    }
}