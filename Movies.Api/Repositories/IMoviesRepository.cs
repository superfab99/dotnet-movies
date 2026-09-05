using Movies.Api.Models;
using Movies.Api.DTOs;

namespace Movies.Api.Repositories
{
    public interface IMoviesRepository : IRepository<Movie>
    {
        Task<(List<MoviesDto> Movies, int TotalCount)> GetMoviesPagedAsync(MovieQueryDto query);
        Task<MoviesDto?> GetMovieWithActorsAsync(int id);
    }
}