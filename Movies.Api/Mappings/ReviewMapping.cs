using AutoMapper;
using Movies.Api.DTOs;
using Movies.Api.Models;

namespace Movies.Api.Mappings
{
    public class ReviewMapping : Profile
    {
        public ReviewMapping()
        {
            CreateMap<Review, ReviewDto>();
            CreateMap<ReviewCreateDto, Review>();
        }
    }
}