
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Review.DTOs;
using Movies.Review.Services;

namespace Movies.Review.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewsService _reviewsService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewsService reviewsService, ILogger<ReviewsController> logger)
        {
            _reviewsService = reviewsService;
            _logger = logger;
        }

        [HttpPost()]
        public async Task<IActionResult> CreateReview([FromBody] ReviewCreateDto reviewCreateDto)
        {
            ReviewDto result;

            try
            {
                result = await _reviewsService.CreateReviewAsync(reviewCreateDto);
            }
            catch (KeyNotFoundException exception)
            {
                _logger.LogWarning(exception.Message);
                return NotFound(exception.Message);
            }

            _logger.LogInformation("Review {ReviewId} created via POST endpoint for movie {MovieId}", result.Id, reviewCreateDto.MovieId);
            return CreatedAtAction(nameof(GetReviewById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewsService.GetReviewByIdAsync(id);

            if (review == null)
            {
                _logger.LogWarning("GET /api/reviews/{id} returned 404 for ReviewId {id}", id, id);
                return NotFound();
            }

            return Ok(review);
        }

        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetReviewsByMovieId(int movieId)
        {
            var reviews = await _reviewsService.GetReviewsByMovieIdAsync(movieId);
            return Ok(reviews);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var isDeleted = await _reviewsService.DeleteReviewAsync(id);
            if (!isDeleted)
            {
                _logger.LogWarning("DELETE /api/reviews/{id} returned 404 for ReviewId {ReviewId}", id, id);
                return NotFound();
            }
            _logger.LogInformation("Review {ReviewId} deleted via DELETE endpoint", id);
            return NoContent();
        }
    }
}