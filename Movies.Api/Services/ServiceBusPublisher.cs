using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Movies.Contracts.Movies;

namespace Movies.Api.Services
{
    public class ServiceBusPublisher : IServiceBusPublisher
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _topicName;
        private readonly ILogger<ServiceBusPublisher> _logger;

        // Constructor: Inject the ServiceBusClient and configuration
        public ServiceBusPublisher(ServiceBusClient serviceBusClient, IConfiguration config, ILogger<ServiceBusPublisher> logger)
        {
            _serviceBusClient = serviceBusClient;
            _topicName = config["AzureServiceBus:TopicName"] ?? "movies-events";
            _logger = logger;
        }

        public async Task PublishMovieCreatedAsync(int movieId, string title, string genre, DateTime releaseDate)
        {
            try
            {
                var movieCreated = new MovieCreated(movieId, title, genre, releaseDate, DateTimeOffset.UtcNow);
                await PublishEventAsync(movieCreated, "MovieCreated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish MovieCreated event");
                throw;
            }
        }

        public async Task PublishMovieDeletedAsync(int movieId)
        {
            try
            {
                var movieDeleted = new MovieDeleted(movieId, DateTimeOffset.UtcNow);
                await PublishEventAsync(movieDeleted, "MovieDeleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish MovieDeleted event");
                throw;
            }
        }

        public async Task PublishMovieUpdatedAsync(int movieId, string title, string genre, DateTime releaseDate)
        {
            try
            {
                var movieUpdated = new MovieUpdated(movieId, title, genre, releaseDate, DateTimeOffset.UtcNow);
                await PublishEventAsync(movieUpdated, "MovieUpdated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish MovieDeleted event");
                throw;
            }
        }

        private async Task PublishEventAsync(object eventData, string eventType)
        {
            //create sender for topic
            var sender = _serviceBusClient.CreateSender(_topicName);
            //conver event to json
            var json = JsonSerializer.Serialize(eventData);

            //create service bus message
            var message = new ServiceBusMessage(json)
            {
                Subject = eventType,
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
        }
    }
}