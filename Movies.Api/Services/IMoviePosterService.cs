using Movies.Api.DTOs;

namespace Movies.Api.Services
{
    public interface IMoviePosterService
    {
        Task<bool> DeletePosterAsync(int posterId);
        Task<MoviePosterDto?> GetMoviePosterAsync(int movieId);
        Task<MoviePosterDto> UploadMoviePosterAsync(int movieId, MoviePosterUploadDto moviePosterUploadDto);
    }
}