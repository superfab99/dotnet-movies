using AutoMapper;
using Movies.Api.Repositories;
using Movies.Review.DTOs;
using Movies.Review.Models;

namespace Movies.Api.Services
{
    public class ReviewsService : IReviewsService
    {

        private readonly IReviewsRepository _reviewRepository;
        private readonly IMapper _mapper;

        public ReviewsService(IReviewsRepository reviewRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto reviewDto)
        {
            // var movie = await _movieRepository.GetByIdAsync(reviewDto.MovieId);

            // if (movie == null)
            // {
            //     _logger.LogWarning("Cannot create review - Movie with ID {MovieId} not found", reviewDto.MovieId);
            //     throw new KeyNotFoundException("The movie was not found.");
            // }

            var review = _mapper.Map<MovieReview>(reviewDto);
            _reviewRepository.Create(review);
            var saved = await _reviewRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new InvalidOperationException("The review could not be created.");
            }

            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return false;
            }

            _reviewRepository.Delete(review);
            var saved = await _reviewRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new InvalidOperationException("The review could not be deleted.");
            }

            return true;
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);

            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<List<ReviewDto>> GetReviewsByMovieIdAsync(int movieId)
        {
            // var movie = await _movieRepository.GetByIdAsync(movieId);

            // if (movie == null)
            // {
            //     throw new KeyNotFoundException("The movie was not found.");
            // }

            var reviews = await _reviewRepository.GetByMovieIdAsync(movieId);
            return reviews.Select(review => _mapper.Map<ReviewDto>(review)).ToList();
        }
    }
}