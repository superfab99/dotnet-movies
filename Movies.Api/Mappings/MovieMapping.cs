using AutoMapper;
using Movies.Api.DTOs;
using Movies.Api.Models;

namespace Movies.Api.Mappings
{
    public class MovieMapping : Profile
    {
        public MovieMapping()
        {
            CreateMap<Movie, MoviesDto>();
            CreateMap<MovieCreateDto, Movie>();
        }
    }
}