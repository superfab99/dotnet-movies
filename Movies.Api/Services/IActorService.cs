using Movies.Api.DTOs;

namespace Movies.Api.Services
{
    public interface IActorService
    {
        Task<ActorDto> CreateActorAsync(ActorCreateDto actorCreateDto);
        Task<List<ActorDto>> GetAllActorsAsync();
        Task<bool> DeleteActorAsync(int id);
        Task<ActorDto?> GetActorAsync(int id);
        Task<ActorDto?> UpdateActorAsync(int id, ActorCreateDto actorCreateDto);
    }
}