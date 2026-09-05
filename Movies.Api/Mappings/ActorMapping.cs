using AutoMapper;
using Movies.Api.DTOs;
using Movies.Api.Models;

namespace Movies.Api.Mappings
{
    public class ActorMapping : Profile
    {
        public ActorMapping()
        {
            CreateMap<Actor, ActorDto>();
            CreateMap<ActorCreateDto, Actor>();
        }
    }
}