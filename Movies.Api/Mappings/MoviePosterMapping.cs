
using AutoMapper;
using Movies.Api.DTOs;
using Movies.Api.Models;

namespace Movies.Api.Mappings
{
    public class MoviePosterMapping : Profile
    {
        public MoviePosterMapping()
        {
            CreateMap<MoviePoster, MoviePosterDto>();
        }
    }
}