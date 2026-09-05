using Movies.Api.Models;
using Movies.Api.DTOs;

namespace Movies.Api.Repositories
{
    public interface IMoviePosterRepository : IRepository<MoviePoster>
    {
        Task<MoviePoster?> GetMoviesPosterAsync(int movieId);
    }
}