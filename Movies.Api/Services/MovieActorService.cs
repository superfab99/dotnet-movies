using Movies.Api.DTOs;
using Movies.Api.Models;
using Movies.Api.Repositories;

namespace Movies.Api.Services;

public class MovieActorService : IMovieActorService
{
    private readonly IRepository<Movie> _movieRepository;
    private readonly IRepository<Actor> _actorRepository;
    private readonly IRepository<MovieActor> _movieActorRepository;

    public MovieActorService(
        IRepository<Movie> movieRepository,
        IRepository<Actor> actorRepository,
        IRepository<MovieActor> movieActorRepository)
    {
        _movieRepository = movieRepository;
        _actorRepository = actorRepository;
        _movieActorRepository = movieActorRepository;
    }

    public async Task<MovieActorDto> AssignActorAsync(
        int movieId,
        MovieActorCreateDto createDto)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId);
        if (movie == null)
        {
            throw new KeyNotFoundException("The movie was not found.");
        }

        var actor = await _actorRepository.GetByIdAsync(createDto.ActorId);
        if (actor == null)
        {
            throw new KeyNotFoundException("The actor was not found.");
        }

        var movieActor = new MovieActor
        {
            MovieId = movieId,
            ActorId = createDto.ActorId,
            CharacterName = createDto.CharacterName,
            IsLeadRole = createDto.IsLeadRole
        };

        _movieActorRepository.Create(movieActor);
        var saved = await _movieActorRepository.SaveChangesAsync();

        if (!saved)
        {
            throw new InvalidOperationException(
                "The actor could not be assigned to the movie.");
        }

        return new MovieActorDto
        {
            MovieId = movieActor.MovieId,
            ActorId = movieActor.ActorId,
            CharacterName = movieActor.CharacterName,
            IsLeadRole = movieActor.IsLeadRole
        };
    }
}
