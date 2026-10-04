using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Movies.Contracts.Movies;
using Movies.Review.Models;
using Movies.Review.Repositories;

namespace Movies.Review.Services
{
    public class ServiceBusConsumer : BackgroundService, IServiceBusConsumer
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ServiceBusConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private ServiceBusProcessor? _processor;

        public ServiceBusConsumer(ServiceBusClient serviceBusClient,
            IConfiguration config,
            ILogger<ServiceBusConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _serviceBusClient = serviceBusClient;
            _config = config;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // This runs when the app starts
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var topicName = _config["AzureServiceBus:TopicName"] ?? "movies-events";
                var subscriptionName = _config["AzureServiceBus:SubscriptionName"] ?? "movies-review-service";

                // Create a processor that listens to the subscription
                _processor = _serviceBusClient.CreateProcessor(topicName, subscriptionName);
                // Register handlers for messages and errors
                _processor.ProcessMessageAsync += ProcessMessageAsync;
                _processor.ProcessErrorAsync += ProcessErrorAsync;

                // Start listening
                await _processor.StartProcessingAsync(stoppingToken);
                _logger.LogInformation($"Service Bus Consumer started for topic '{topicName}', subscription '{subscriptionName}'");

                // Keep the service running
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Service Bus Consumer stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service Bus Consumer error");
                throw;
            }
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var json = args.Message.Body.ToString();
                var eventType = args.Message.Subject;

                _logger.LogInformation($"Processing event: {eventType}");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var moviesRepository = scope.ServiceProvider.GetRequiredService<IMoviesRepository>();

                    if (eventType == "MovieCreated")
                    {
                        var movieCreated = JsonSerializer.Deserialize<MovieCreated>(json);
                        if (movieCreated != null)
                        {
                            await HandleMovieCreatedAsync(movieCreated, moviesRepository);
                        }
                    }
                    else if (eventType == "MovieUpdated")
                    {
                        var movieUpdated = JsonSerializer.Deserialize<MovieUpdated>(json);
                        if (movieUpdated != null)
                        {
                            await HandleMovieUpdatedAsync(movieUpdated, moviesRepository);
                        }
                    }
                    else if (eventType == "MovieDeleted")
                    {
                        var movieDeleted = JsonSerializer.Deserialize<MovieDeleted>(json);
                        if (movieDeleted != null)
                        {
                            await HandleMovieDeletedAsync(movieDeleted, moviesRepository);
                        }
                    }
                }

                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Service Bus message");
                // Don't complete the message - it will be retried
                // After max retries, it goes to Dead Letter Queue
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, $"Service Bus error: {args.ErrorSource}");
            return Task.CompletedTask;
        }

        private async Task HandleMovieCreatedAsync(MovieCreated movieCreated, IMoviesRepository repository)
        {
            var movie = new Movie
            {
                SourceMovieId = movieCreated.MovieId,
                Title = movieCreated.Title,
                Genre = movieCreated.Genre,
                ReleaseDate = movieCreated.ReleaseDate
            };

            repository.Create(movie);
            await repository.SaveChangesAsync();

            _logger.LogInformation($"Movie {movieCreated.MovieId} saved to review database");
        }

        private async Task HandleMovieUpdatedAsync(MovieUpdated movieUpdated, IMoviesRepository repository)
        {
            var movie = await repository.GetBySourceMovieIdAsync(movieUpdated.MovieId);
            if (movie == null)
            {
                _logger.LogWarning($"Movie {movieUpdated.MovieId} not found for update");
                return;
            }

            movie.Title = movieUpdated.Title;
            movie.Genre = movieUpdated.Genre;
            movie.ReleaseDate = movieUpdated.ReleaseDate;

            await repository.SaveChangesAsync();
            _logger.LogInformation($"Movie {movieUpdated.MovieId} updated in review database");
        }

        private async Task HandleMovieDeletedAsync(MovieDeleted movieDeleted, IMoviesRepository repository)
        {
            await repository.DeleteBySourceMovieIdAsync(movieDeleted.MovieId);
            _logger.LogInformation($"Movie {movieDeleted.MovieId} deleted from review database");
        }

        // Cleanup when app stops
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processor != null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }

    }
}