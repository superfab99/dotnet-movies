
using MassTransit;
using Movies.Contracts.Movies;
using Movies.Review.Repositories;

namespace Movies.Review.Consumers
{
    public sealed class MovieUpdatedConsumer : IConsumer<MovieUpdated>
    {
        private readonly ILogger<MovieUpdatedConsumer> _logger;
        private readonly IMoviesRepository _movieRepository;

        public MovieUpdatedConsumer(ILogger<MovieUpdatedConsumer> logger, IMoviesRepository movieRepository)
        {
            _logger = logger;
            _movieRepository = movieRepository;
        }
        public async Task Consume(ConsumeContext<MovieUpdated> context)
        {
            var message = context.Message;
            var isUpdated = await _movieRepository
                .UpdateMovieBySourceMovieIdAsync(message.MovieId, message);

            if (!isUpdated)
            {
                _logger.LogInformation(
                    "Failed to update movie {MovieId} from the review database.",
                    message.MovieId);
                return;
            }

            _logger.LogInformation(
               "Movie {MovieId} updated in the review database.",
               message.MovieId);
        }
    }
}