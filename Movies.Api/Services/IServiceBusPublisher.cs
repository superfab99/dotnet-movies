namespace Movies.Api.Services
{
    public interface IServiceBusPublisher
    {
        Task PublishMovieCreatedAsync(int movieId, string title, string genre, DateTime releaseDate);
        Task PublishMovieUpdatedAsync(int movieId, string title, string genre, DateTime releaseDate);
        Task PublishMovieDeletedAsync(int movieId);
    }
}