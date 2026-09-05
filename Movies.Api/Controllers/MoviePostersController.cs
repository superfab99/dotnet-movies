using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.DTOs;
using Movies.Api.Services;

namespace Movies.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/movies/{movieId:int}/poster")]
    public class MoviePostersController : ControllerBase
    {
        private readonly IMoviePosterService _moviePosterService;
        private readonly ILogger<MoviePostersController> _logger;

        public MoviePostersController(
            IMoviePosterService moviePosterService,
            ILogger<MoviePostersController> logger)
        {
            _moviePosterService = moviePosterService;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMoviePoster(
            int movieId,
            [FromForm] MoviePosterUploadDto uploadDto)
        {
            var poster = await _moviePosterService.UploadMoviePosterAsync(
                movieId,
                uploadDto);

            _logger.LogInformation(
                "Poster {PosterId} created for MovieId {MovieId}",
                poster.Id,
                movieId);

            return CreatedAtAction(
                nameof(GetMoviePoster),
                new { movieId },
                poster);
        }

        [HttpGet]
        public async Task<IActionResult> GetMoviePoster(int movieId)
        {
            var poster = await _moviePosterService.GetMoviePosterAsync(movieId);

            if (poster == null)
            {
                _logger.LogWarning(
                    "GET /api/movies/{MovieId}/poster returned 404",
                    movieId);
                return NotFound();
            }

            return Ok(poster);
        }

        [HttpDelete("~/api/movie-posters/{posterId:int}")]
        public async Task<IActionResult> DeleteMoviePoster(int posterId)
        {
            var deleted = await _moviePosterService.DeletePosterAsync(posterId);

            if (!deleted)
            {
                _logger.LogWarning(
                    "DELETE /api/movie-posters/{PosterId} returned 404",
                    posterId);
                return NotFound();
            }

            _logger.LogInformation(
                "Poster {PosterId} deleted",
                posterId);

            return NoContent();
        }
    }
}
