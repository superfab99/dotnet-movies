
using MassTransit;
using Movies.Contracts.Movies;
using Movies.Review.Repositories;

namespace Movies.Review.Consumers
{
    public sealed class MovieDeletedConsumer : IConsumer<MovieDeleted>
    {
        private readonly ILogger<MovieDeletedConsumer> _logger;
        private readonly IMoviesRepository _movieRepository;

        public MovieDeletedConsumer(ILogger<MovieDeletedConsumer> logger, IMoviesRepository movieRepository)
        {
            _logger = logger;
            _movieRepository = movieRepository;
        }

        public async Task Consume(ConsumeContext<MovieDeleted> context)
        {
            var message = context.Message;
            var isDeleted = await _movieRepository
                .DeleteBySourceMovieIdAsync(message.MovieId);

            if (!isDeleted)
            {
                _logger.LogInformation(
                    "Movie {MovieId} was already absent from the review database.",
                    message.MovieId);
                return;
            }

            _logger.LogInformation(
               "Movie {MovieId} deleted from the review database.",
               message.MovieId);
        }
    }
}