using Microsoft.EntityFrameworkCore;
using Movies.Api.Data;
using Movies.Api.DTOs;
using Movies.Api.Models;

namespace Movies.Api.Repositories
{
    public class MoviePosterRepository : Repository<MoviePoster>, IMoviePosterRepository
    {
        private readonly ILogger<MoviePosterRepository> _logger;

        public MoviePosterRepository(MoviesApiDbContext dbContext, ILogger<MoviePosterRepository> logger) : base(dbContext)
        {
            _logger = logger;
        }


        public async Task<MoviePoster?> GetMoviesPosterAsync(int movieId)
        {
            var result = await _dbSet.FirstOrDefaultAsync(mp => mp.MovieId == movieId);
            return result;
        }
    }
}