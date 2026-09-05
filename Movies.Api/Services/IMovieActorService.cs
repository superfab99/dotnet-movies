using Movies.Api.DTOs;

namespace Movies.Api.Services;

public interface IMovieActorService
{
    Task<MovieActorDto> AssignActorAsync(int movieId, MovieActorCreateDto createDto);
}
