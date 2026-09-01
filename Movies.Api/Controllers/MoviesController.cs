using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Movies.Api.DTOs;
using Movies.Api.Services;

namespace Movies.Api.Controller
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMoviesService _moviesService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(IMoviesService moviesService, ILogger<MoviesController> logger)
        {
            _moviesService = moviesService;
            _logger = logger;
        }

        [HttpPost()]
        public async Task<IActionResult> CreateMovie([FromBody] MovieCreateDto movieCreateDto)
        {
            var movie = await _moviesService.CreateMovieAsync(movieCreateDto);
            _logger.LogInformation("Movie {MovieId} created via POST endpoint", movie.Id);
            return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);
        }

        [HttpGet()]
        [EnableRateLimiting("movieslimit")]
        public async Task<IActionResult> GetAllMovies([FromQuery] MovieQueryDto query)
        {
            _logger.LogInformation("GetAllMovies api was called");
            if (query.PageNumber < 1 || query.PageSize < 1 || query.PageSize > 100)
                return BadRequest("pageNumber must be at least 1 and pageSize must be between 1 and 100.");

            var movies = await _moviesService.GetAllMoviesAsync(query);
            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(int id)
        {
            var movie = await _moviesService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                _logger.LogWarning("GET /api/movies/{id} returned 404 for MovieId {MovieId}", id, id);
                return NotFound();
            }
            return Ok(movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] MovieUpdateDto movieUpdateDto)
        {
            var updatedMovie = await _moviesService.UpdateMovieAsync(id, movieUpdateDto);
            if (updatedMovie == null)
            {
                _logger.LogWarning("PUT /api/movies/{id} returned 404 for MovieId {MovieId}", id, id);
                return NotFound();
            }
            _logger.LogInformation("Movie {MovieId} updated via PUT endpoint", id);
            return Ok(updatedMovie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var deleted = await _moviesService.DeleteMovieAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("DELETE /api/movies/{id} returned 404 for MovieId {MovieId}", id, id);
                return NotFound();
            }
            _logger.LogInformation("Movie {MovieId} deleted via DELETE endpoint", id);
            return NoContent();
        }

    }
}