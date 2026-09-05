using Microsoft.EntityFrameworkCore;
using Movies.Api.Data;
using Movies.Api.DTOs;
using Movies.Api.Models;

namespace Movies.Api.Repositories
{
    public class MoviesRepository : Repository<Movie>, IMoviesRepository
    {
        private readonly ILogger<MoviesRepository> _logger;

        public MoviesRepository(MoviesApiDbContext dbContext, ILogger<MoviesRepository> logger) : base(dbContext)
        {
            _logger = logger;
        }
        public async Task<(List<MoviesDto> Movies, int TotalCount)> GetMoviesPagedAsync(MovieQueryDto query)
        {
            var movieQuery = _dbContext.Movies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Title))
                movieQuery = movieQuery.Where(movie => movie.Title.Contains(query.Title));

            if (!string.IsNullOrWhiteSpace(query.Genre))
                movieQuery = movieQuery.Where(movie => movie.Genre.Contains(query.Genre));

            var totalCount = await movieQuery.CountAsync();

            var orderedQuery = query.SortBy.Trim().ToLowerInvariant() switch
            {
                "title" => query.Descending
                    ? movieQuery.OrderByDescending(movie => movie.Title)
                    : movieQuery.OrderBy(movie => movie.Title),
                "genre" => query.Descending
                    ? movieQuery.OrderByDescending(movie => movie.Genre)
                    : movieQuery.OrderBy(movie => movie.Genre),
                "rating" => query.Descending
                    ? movieQuery.OrderByDescending(movie => movie.Rating)
                    : movieQuery.OrderBy(movie => movie.Rating),
                "releasedate" => query.Descending
                    ? movieQuery.OrderByDescending(movie => movie.ReleaseDate)
                    : movieQuery.OrderBy(movie => movie.ReleaseDate),
                "durationminutes" => query.Descending
                    ? movieQuery.OrderByDescending(movie => movie.DurationMinutes)
                    : movieQuery.OrderBy(movie => movie.DurationMinutes),
                _ => query.Descending
                    ? movieQuery.OrderByDescending(movie => movie.Id)
                    : movieQuery.OrderBy(movie => movie.Id)
            };

            var movies = await orderedQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(movie => new MoviesDto
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    Description = movie.Description,
                    ReleaseDate = movie.ReleaseDate,
                    DurationMinutes = movie.DurationMinutes,
                    Genre = movie.Genre,
                    Rating = movie.Rating,
                    CreatedAt = movie.CreatedAt,
                    ModifiedAt = movie.ModifiedAt,
                    MoviePoster = movie.MoviePoster == null
                        ? null
                        : new MoviePosterDto
                        {
                            Id = movie.MoviePoster.Id,
                            AltText = movie.MoviePoster.AltText,
                            ImageUrl = movie.MoviePoster.ImageUrl,
                            MovieId = movie.MoviePoster.MovieId,
                        }
                })
                .ToListAsync();

            _logger.LogInformation("GetMoviesPagedAsync returned {ReturnedCount} movies (Page {PageNumber})",
                movies.Count, query.PageNumber);

            return (movies, totalCount);
        }
    }
}