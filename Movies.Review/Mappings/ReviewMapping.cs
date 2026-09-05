
using AutoMapper;
using Movies.Review.DTOs;
using Movies.Review.Models;

namespace Movies.Review.Mappings
{
    public class ReviewMapping : Profile
    {
        public ReviewMapping()
        {
            CreateMap<MovieReview, ReviewDto>();
            CreateMap<ReviewCreateDto, MovieReview>();
        }
    }
}