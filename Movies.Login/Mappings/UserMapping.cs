using AutoMapper;
using Movies.Login.DTOs;
using Movies.Login.Models;

namespace Movies.Login.Mappings
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<User, UserReadDto>();
            CreateMap<UserRegisterDto, User>();
        }
    }
}