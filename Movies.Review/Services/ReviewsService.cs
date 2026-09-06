using AutoMapper;
using Movies.Review.DTOs;
using Movies.Review.Models;
using Movies.Review.Repositories;

namespace Movies.Review.Services
{
    public class ReviewsService : IReviewsService
    {

        private readonly IReviewsRepository _reviewRepository;
        private readonly IMoviesRepository _moviesRepository;
        private readonly IMapper _mapper;

        public ReviewsService(
            IReviewsRepository reviewRepository,
            IMoviesRepository moviesRepository,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _moviesRepository = moviesRepository;
            _mapper = mapper;
        }

        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto reviewDto)
        {
            var movie = await _moviesRepository
                .GetBySourceMovieIdAsync(reviewDto.MovieId);

            if (movie is null)
            {
                throw new KeyNotFoundException(
                    $"Movie {reviewDto.MovieId} was not found.");
            }

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