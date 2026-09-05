using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Movies.Api.DTOs;
using Movies.Api.Models;
using Movies.Api.Repositories;

namespace Movies.Api.Services
{
    public class MoviesService : IMoviesService
    {
        private const int MaxPageSize = 100;

        private readonly IMoviesRepository _moviesRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MoviesService> _logger;
        private readonly IMemoryCache _cache;
        private const string MoviesCacheVersionKey = "movies:version";

        public MoviesService(IMoviesRepository moviesRepository,
        IMapper mapper, ILogger<MoviesService> logger,
        IMemoryCache cache)
        {
            _moviesRepository = moviesRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<MoviesDto> CreateMovieAsync(MovieCreateDto movieCreateDto)
        {
            var movie = _mapper.Map<Movie>(movieCreateDto);
            _moviesRepository.Create(movie);
            var saved = await _moviesRepository.SaveChangesAsync();

            if (!saved)
            {
                _logger.LogError("Failed to create movie with title '{Title}'", movieCreateDto.Title);
                throw new InvalidOperationException("The movie could not be created.");
            }
            var currentVersion = _cache.Get<int>(MoviesCacheVersionKey);
            _cache.Set(MoviesCacheVersionKey, currentVersion + 1, TimeSpan.FromHours(24));
            _logger.LogInformation("Movie created successfully with ID {MovieId}, Title '{Title}'", movie.Id, movie.Title);
            return _mapper.Map<MoviesDto>(movie);
        }

        public async Task<PagedResultDto<MoviesDto>> GetAllMoviesAsync(MovieQueryDto query)
        {
            if (query.PageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(query.PageNumber), "Page number must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > MaxPageSize)
                throw new ArgumentOutOfRangeException(nameof(query.PageSize), $"Page size must be between 1 and {MaxPageSize}.");

            var version = _cache.GetOrCreate(MoviesCacheVersionKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                return 1;
            });

            var cacheKey = $"movies:v{version}:{query.PageNumber}:{query.PageSize}:{query.Title ?? "all"}:{query.Genre ?? "all"}:{query.SortBy ?? "none"}:{query.Descending}";
            if (_cache.TryGetValue(cacheKey, out var cachedMovies) && cachedMovies is PagedResultDto<MoviesDto> cachedPagedResult)
            {
                return cachedPagedResult;
            }

            var result = await _moviesRepository.GetMoviesPagedAsync(query);

            if (result.Movies.Count == 0)
                _logger.LogWarning("No movies found for query - Title: {Title}, Genre: {Genre}", query.Title ?? "null", query.Genre ?? "null");
            else
                _logger.LogInformation("Retrieved {ReturnedCount} movies out of {TotalCount} total", result.Movies.Count, result.TotalCount);

            var item = new PagedResultDto<MoviesDto>
            {
                Data = result.Movies,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = (int)Math.Ceiling(result.TotalCount / (double)query.PageSize)
            };
            _cache.Set(cacheKey, item, TimeSpan.FromMinutes(10));
            return item;
        }

        public async Task<MoviesDto?> GetMovieByIdAsync(int id)
        {
            var movie = await _moviesRepository.GetMovieWithActorsAsync(id);

            if (movie == null)
            {
                _logger.LogWarning("Movie with ID {MovieId} not found", id);
                return null;
            }

            return movie;
        }

        public async Task<MoviesDto?> UpdateMovieAsync(int id, MovieUpdateDto movieUpdateDto)
        {
            var movie = await _moviesRepository.GetByIdAsync(id);

            if (movie == null)
            {
                _logger.LogWarning("Movie with ID {MovieId} not found for update", id);
                return null;
            }

            movie.Title = movieUpdateDto.Title;
            movie.Description = movieUpdateDto.Description;
            movie.DurationMinutes = movieUpdateDto.DurationMinutes;
            movie.Genre = movieUpdateDto.Genre;
            movie.Rating = movieUpdateDto.Rating;
            movie.ReleaseDate = movieUpdateDto.ReleaseDate;

            await _moviesRepository.SaveChangesAsync();
            var currentVersion = _cache.Get<int>(MoviesCacheVersionKey);
            _cache.Set(MoviesCacheVersionKey, currentVersion + 1, TimeSpan.FromHours(24));

            _logger.LogInformation("Movie with ID {MovieId} updated successfully", id);
            return _mapper.Map<MoviesDto>(movie);
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _moviesRepository.GetByIdAsync(id);
            if (movie == null)
            {
                _logger.LogWarning("Movie with ID {MovieId} not found for deletion", id);
                return false;
            }

            var deleted = await _moviesRepository.DeleteAsync(id);
            if (!deleted)
                return false;

            var saved = await _moviesRepository.SaveChangesAsync();

            if (!saved)
            {
                _logger.LogError("Failed to save deletion for movie with ID {MovieId}", id);
                throw new InvalidOperationException("The movie could not be deleted.");
            }

            var currentVersion = _cache.Get<int>(MoviesCacheVersionKey);
            _cache.Set(MoviesCacheVersionKey, currentVersion + 1, TimeSpan.FromHours(24));

            _logger.LogInformation("Movie with ID {MovieId} deleted successfully", id);
            return true;
        }
    }
}