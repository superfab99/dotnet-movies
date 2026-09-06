using MassTransit;
using Movies.Contracts.Movies;
using Movies.Review.Models;
using Movies.Review.Repositories;

namespace Movies.Review.Consumers
{
    public sealed class MovieCreatedConsumer : IConsumer<MovieCreated>
    {
        private readonly ILogger<MovieCreatedConsumer> _logger;
        private readonly IMoviesRepository _moviesRepository;

        public MovieCreatedConsumer(ILogger<MovieCreatedConsumer> logger, IMoviesRepository moviesRepository)
        {
            _logger = logger;
            _moviesRepository = moviesRepository;
        }

        public async Task Consume(ConsumeContext<MovieCreated> context)
        {
            var message = context.Message;

            var existingMovie = await _moviesRepository
                .GetBySourceMovieIdAsync(message.MovieId);

            if (existingMovie is not null)
            {
                _logger.LogInformation(
                    "Movie {MovieId} already exists in the review database.",
                    message.MovieId);
                return;
            }

            var movie = new Movie
            {
                Genre = message.Genre,
                ReleaseDate = message.ReleaseDate,
                SourceMovieId = message.MovieId,
                Title = message.Title,
            };
            _moviesRepository.Create(movie);

            var saved = await _moviesRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new InvalidOperationException(
                    $"Movie {message.MovieId} could not be saved.");
            }

            _logger.LogInformation(
                "Movie {MovieId} saved to the review database.",
                message.MovieId);
        }
    }
}