using AutoMapper;
using Movies.Api.DTOs;
using Movies.Api.Models;
using Movies.Api.Repositories;

namespace Movies.Api.Services
{
    public class ActorService : IActorService
    {
        private readonly IMapper _mapper;
        private readonly IActorRepository _actorRepository;
        public ActorService(IMapper mapper, IActorRepository actorRepository)
        {
            _mapper = mapper;
            _actorRepository = actorRepository;
        }

        public async Task<ActorDto> CreateActorAsync(ActorCreateDto actorCreateDto)
        {
            var actor = _mapper.Map<Actor>(actorCreateDto);
            _actorRepository.Create(actor);
            var saved = await _actorRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new InvalidOperationException("The actor could not be created.");
            }

            return _mapper.Map<ActorDto>(actor);
        }

        public async Task<List<ActorDto>> GetAllActorsAsync()
        {
            var actors = await _actorRepository.GetAllAsync();
            return _mapper.Map<List<ActorDto>>(actors);
        }

        public async Task<ActorDto?> GetActorAsync(int id)
        {
            var actor = await _actorRepository.GetByIdAsync(id);
            if (actor == null)
            {
                return null;
            }
            return _mapper.Map<ActorDto>(actor);
        }

        public async Task<bool> DeleteActorAsync(int id)
        {
            var actor = await _actorRepository.GetByIdAsync(id);
            if (actor == null)
            {
                return false;
            }

            _actorRepository.Delete(actor);
            return await _actorRepository.SaveChangesAsync();
        }

        public async Task<ActorDto?> UpdateActorAsync(int id, ActorCreateDto actorCreateDto)
        {
            var actor = await _actorRepository.GetByIdAsync(id);
            if (actor == null)
            {
                return null;
            }

            actor.FirstName = actorCreateDto.FirstName;
            actor.LastName = actorCreateDto.LastName;
            actor.Gender = actorCreateDto.Gender;
            actor.ProfileImageUrl = actorCreateDto.ProfileImageUrl;

            _actorRepository.Update(actor);
            var saved = await _actorRepository.SaveChangesAsync();

            if (!saved)
            {
                throw new InvalidOperationException("The actor could not be updated.");
            }

            return _mapper.Map<ActorDto>(actor);
        }
    }
}