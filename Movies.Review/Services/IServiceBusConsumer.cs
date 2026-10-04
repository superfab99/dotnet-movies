namespace Movies.Review.Services
{
    public interface IServiceBusConsumer
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}