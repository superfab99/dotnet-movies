using Movies.Api.DTOs;

namespace Movies.Api.Services
{
    public interface IMoviesService
    {
        Task<MoviesDto> CreateMovieAsync(MovieCreateDto movieCreateDto);
        Task<PagedResultDto<MoviesDto>> GetAllMoviesAsync(MovieQueryDto query);
        Task<MoviesDto?> GetMovieByIdAsync(int id);
        Task<MoviesDto?> UpdateMovieAsync(int id, MovieUpdateDto movieUpdateDto);
        Task<bool> DeleteMovieAsync(int id);
    }
}