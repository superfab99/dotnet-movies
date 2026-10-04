# Azure Service Bus Topics - Learning Guide with Code Snippets

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│ PUBLISHER (Movies.Api)                                      │
│ ├─ MovieCreated event                                       │
│ ├─ MovieUpdated event                                       │
│ └─ MovieDeleted event                                       │
│         ↓ (sends to)                                        │
└─────────────────────────────────────────────────────────────┘
                    Topic: "movies-events"
┌─────────────────────────────────────────────────────────────┐
│ SUBSCRIBERS (receive from)                                   │
│ Subscription: "movies-review-service"                       │
│     └─ Movies.Review Service consumes                       │
│         ├─ MovieCreated → Process new review DB entry      │
│         ├─ MovieUpdated → Update review DB                 │
│         └─ MovieDeleted → Delete from review DB            │
└─────────────────────────────────────────────────────────────┘
```

---

## Step 1: Add NuGet Package (ALREADY DONE ✓)

```bash
dotnet add package Azure.Messaging.ServiceBus --version 7.18.0
```

---

## Step 2: Add Configuration to appsettings.json

**File: `appsettings.Deve/tmp/azure_service_bus_topic_guide.mdlopment.json` and `appsettings.json`**

```json
{
  "AzureServiceBus": {
    "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_KEY",
    "TopicName": "movies-events",
    "SubscriptionName": "movies-review-service"
  }
}
```

**Where to get these values:**

- Go to Azure Portal → Service Bus Namespace → Shared access policies
- Copy the connection string
- Create a topic called "movies-events"
- Create a subscription called "movies-review-service"

---

## Step 3: Create Publisher Service (Movies.Api)

### **Interface** (IServiceBusPublisher.cs)

```csharp
namespace Movies.Api.Services
{
    // This interface defines what events we'll publish to Azure Service Bus
    public interface IServiceBusPublisher
    {
        // When a movie is created, publish this event
        Task PublishMovieCreatedAsync(int movieId, string title, string genre, DateTime releaseDate);

        // When a movie is updated, publish this event
        Task PublishMovieUpdatedAsync(int movieId, string title, string genre, DateTime releaseDate);

        // When a movie is deleted, publish this event
        Task PublishMovieDeletedAsync(int movieId);
    }
}
```

### **Implementation** (ServiceBusPublisher.cs)

```csharp
using Azure.Messaging.ServiceBus;
using Movies.Contracts.Movies;
using System.Text.Json;

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
                // Create an event object
                var movieCreated = new MovieCreated(movieId, title, genre, releaseDate, DateTime.UtcNow);

                // Send it to the topic
                await PublishEventAsync(movieCreated, "MovieCreated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish MovieCreated event");
                throw;
            }
        }

        public async Task PublishMovieUpdatedAsync(int movieId, string title, string genre, DateTime releaseDate)
        {
            try
            {
                var movieUpdated = new MovieUpdated(movieId, title, genre, releaseDate, DateTime.UtcNow);
                await PublishEventAsync(movieUpdated, "MovieUpdated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish MovieUpdated event");
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

        // Helper method to send any event to the topic
        private async Task PublishEventAsync(object eventData, string eventType)
        {
            // Step 1: Create a sender for this topic
            var sender = _serviceBusClient.CreateSender(_topicName);

            // Step 2: Convert the event to JSON
            var json = JsonSerializer.Serialize(eventData);

            // Step 3: Create a Service Bus message
            var message = new ServiceBusMessage(json)
            {
                Subject = eventType, // Label this message with event type
                ContentType = "application/json"
            };

            // Step 4: Send the message to the topic
            await sender.SendMessageAsync(message);

            _logger.LogInformation($"Published {eventType} event to topic '{_topicName}'");
        }
    }
}
```

---

## Step 4: Update Program.cs (Movies.Api)

### **Add to Program.cs - Configuration Section**

```csharp
// Add Azure Service Bus client
var serviceBusConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));

// Register the publisher service
builder.Services.AddScoped<IServiceBusPublisher, ServiceBusPublisher>();
```

---

## Step 5: Update MoviesService.cs (Movies.Api)

### **Replace IPublishEndpoint with IServiceBusPublisher**

**BEFORE (with MassTransit):**

```csharp
private readonly IPublishEndpoint _publishEndpoint;

public async Task<MoviesDto> CreateMovieAsync(MovieCreateDto movieCreateDto)
{
    // ... create movie ...
    await _publishEndpoint.Publish(new MovieCreated(...));
}
```

**AFTER (with Azure Service Bus):**

```csharp
private readonly IServiceBusPublisher _serviceBusPublisher;

public async Task<MoviesDto> CreateMovieAsync(MovieCreateDto movieCreateDto)
{
    var movie = _mapper.Map<Movie>(movieCreateDto);
    _moviesRepository.Create(movie);
    var saved = await _moviesRepository.SaveChangesAsync();

    if (!saved)
    {
        _logger.LogError("Failed to create movie");
        throw new InvalidOperationException("The movie could not be created.");
    }

    // Publish to Service Bus Topic
    await _serviceBusPublisher.PublishMovieCreatedAsync(
        movie.Id,
        movie.Title,
        movie.Genre,
        movie.ReleaseDate
    );

    _logger.LogInformation($"Movie {movie.Id} created and published");
    return _mapper.Map<MoviesDto>(movie);
}

public async Task<MoviesDto?> UpdateMovieAsync(int id, MovieUpdateDto movieUpdateDto)
{
    var movie = await _moviesRepository.GetByIdAsync(id);
    if (movie == null) return null;

    movie.Title = movieUpdateDto.Title;
    movie.Genre = movieUpdateDto.Genre;
    // ... other updates ...

    await _moviesRepository.SaveChangesAsync();

    // Publish to Service Bus Topic
    await _serviceBusPublisher.PublishMovieUpdatedAsync(
        movie.Id,
        movie.Title,
        movie.Genre,
        movie.ReleaseDate
    );

    return _mapper.Map<MoviesDto>(movie);
}

public async Task<bool> DeleteMovieAsync(int id)
{
    var movie = await _moviesRepository.GetByIdAsync(id);
    if (movie == null) return false;

    var deleted = await _moviesRepository.DeleteAsync(id);
    if (!deleted) return false;

    var saved = await _moviesRepository.SaveChangesAsync();
    if (!saved) throw new InvalidOperationException("Failed to delete movie");

    // Publish to Service Bus Topic
    await _serviceBusPublisher.PublishMovieDeletedAsync(movie.Id);

    return true;
}
```

---

## Step 6: Create Consumer Service (Movies.Review)

### **Hosted Service** (ServiceBusConsumer.cs)

```csharp
using Azure.Messaging.ServiceBus;
using Movies.Contracts.Movies;
using Movies.Review.Repositories;
using Movies.Review.Models;
using System.Text.Json;

namespace Movies.Review.Services
{
    // This is a "Hosted Service" - it runs in the background when the app starts
    public class ServiceBusConsumer : BackgroundService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ServiceBusConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private ServiceBusProcessor? _processor;

        public ServiceBusConsumer(
            ServiceBusClient serviceBusClient,
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

        // Handle incoming messages
        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var json = args.Message.Body.ToString();
                var eventType = args.Message.Subject; // e.g., "MovieCreated"

                _logger.LogInformation($"Processing event: {eventType}");

                // Using DI to get repositories
                using (var scope = _serviceProvider.CreateScope())
                {
                    var moviesRepository = scope.ServiceProvider.GetRequiredService<IMoviesRepository>();

                    // Route based on event type
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

                // Mark the message as processed
                await args.CompleteMessageAsync(args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Service Bus message");
                // Don't complete the message - it will be retried
                // After max retries, it goes to Dead Letter Queue
            }
        }

        // Handle errors
        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, $"Service Bus error: {args.ErrorSource}");
            return Task.CompletedTask;
        }

        // Event Handlers
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
            await repository.DeleteAsync(movieDeleted.MovieId);
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
```

---

## Step 7: Register Consumer in Program.cs (Movies.Review)

### **Add to Program.cs**

```csharp
// Add Azure Service Bus client
var serviceBusConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));

// Register the consumer as a background service
builder.Services.AddHostedService<ServiceBusConsumer>();
```

---

## Step 8: Interface for Consumer (Movies.Review)

Create an interface so you can easily swap implementations:

```csharp
namespace Movies.Review.Services
{
    public interface IServiceBusConsumer
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
```

---

## Summary of Changes Needed

| Component         | File             | Change                                           |
| ----------------- | ---------------- | ------------------------------------------------ |
| **Movies.Api**    | appsettings.json | Add AzureServiceBus config                       |
| **Movies.Api**    | Program.cs       | Register ServiceBusClient + IServiceBusPublisher |
| **Movies.Api**    | Services/        | Create IServiceBusPublisher interface            |
| **Movies.Api**    | Services/        | Create ServiceBusPublisher implementation        |
| **Movies.Api**    | MoviesService.cs | Inject IServiceBusPublisher, call methods        |
| **Movies.Review** | appsettings.json | Add AzureServiceBus config                       |
| **Movies.Review** | Program.cs       | Register ServiceBusClient + ServiceBusConsumer   |
| **Movies.Review** | Services/        | Create ServiceBusConsumer (BackgroundService)    |
| **Movies.Review** | Services/        | Create event handler methods                     |

---

## Key Concepts to Understand

### **Publisher Pattern (Movies.Api)**

- **What:** Sends events to a Topic
- **How:** ServiceBusClient → CreateSender(topicName) → SendMessageAsync()
- **When:** After creating/updating/deleting a movie

### **Subscriber Pattern (Movies.Review)**

- **What:** Listens to events from a Subscription
- **How:** ServiceBusClient → CreateProcessor(topicName, subscriptionName)
- **When:** BackgroundService runs continuously while app is running

### **Message Flow**

```
1. Movie created in Movies.Api
2. PublishMovieCreatedAsync() called
3. ServiceBusPublisher sends to Topic
4. Topic routes to Subscription
5. ServiceBusProcessor receives message
6. ProcessMessageAsync() triggered
7. HandleMovieCreatedAsync() updates review database
```

### **Error Handling**

- If handler throws exception → message NOT completed
- Service Bus retries automatically (up to MaxDeliveryCount = 10)
- After max retries → moves to Dead Letter Queue
- You can monitor DLQ for failed messages

---

## Testing Checklist

- [ ] Create ServiceBusPublisher
- [ ] Create ServiceBusConsumer
- [ ] Register in Program.cs (both projects)
- [ ] Add configuration to appsettings
- [ ] Update MoviesService to use publisher
- [ ] Run both services
- [ ] Create a movie in Movies.Api
- [ ] Check Movies.Review database for new entry
- [ ] Update a movie, verify update in Movies.Review
- [ ] Delete a movie, verify deletion in Movies.Review
