using AutoMapper;
using Movies.Api.DTOs;
using Movies.Api.Models;
using Movies.Api.Repositories;

namespace Movies.Api.Services
{
    public class ReviewsService : IReviewsService
    {

        private readonly IReviewsRepository _reviewRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewsService> _logger;

        public ReviewsService(
            IReviewsRepository reviewRepository,
            IRepository<Movie> movieRepository,
            IMapper mapper,
            ILogger<ReviewsService> logger)
        {
            _reviewRepository = reviewRepository;
            _movieRepository = movieRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto reviewDto)
        {
            var movie = await _movieRepository.GetByIdAsync(reviewDto.MovieId);

            if (movie == null)
            {
                _logger.LogWarning("Cannot create review - Movie with ID {MovieId} not found", reviewDto.MovieId);
                throw new KeyNotFoundException("The movie was not found.");
            }

            var review = _mapper.Map<Review>(reviewDto);
            _reviewRepository.Create(review);
            var saved = await _reviewRepository.SaveChangesAsync();

            if (!saved)
            {
                _logger.LogError("Failed to save review for movie {MovieId}", reviewDto.MovieId);
                throw new InvalidOperationException("The review could not be created.");
            }

            _logger.LogInformation("Review created successfully - ID: {ReviewId}, MovieId: {MovieId}", review.Id, reviewDto.MovieId);
            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                _logger.LogWarning("Review with ID {ReviewId} not found for deletion", id);
                return false;
            }

            _reviewRepository.Delete(review);
            var saved = await _reviewRepository.SaveChangesAsync();

            if (!saved)
            {
                _logger.LogError("Failed to save deletion for review with ID {ReviewId}", id);
                throw new InvalidOperationException("The review could not be deleted.");
            }

            _logger.LogInformation("Review with ID {ReviewId} deleted successfully", id);
            return true;
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                _logger.LogWarning("Review with ID {ReviewId} not found", id);

            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<List<ReviewDto>> GetReviewsByMovieIdAsync(int movieId)
        {
            var movie = await _movieRepository.GetByIdAsync(movieId);

            if (movie == null)
            {
                _logger.LogWarning("Cannot fetch reviews - Movie with ID {MovieId} not found", movieId);
                throw new KeyNotFoundException("The movie was not found.");
            }

            var reviews = await _reviewRepository.GetByMovieIdAsync(movieId);

            if (reviews.Count == 0)
                _logger.LogWarning("No reviews found for movie {MovieId}", movieId);
            else
                _logger.LogInformation("Retrieved {ReviewCount} reviews for movie {MovieId}", reviews.Count, movieId);

            return reviews.Select(review => _mapper.Map<ReviewDto>(review)).ToList();
        }
    }
}