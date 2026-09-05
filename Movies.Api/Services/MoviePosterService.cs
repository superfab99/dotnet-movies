using AutoMapper;
using Movies.Api.DTOs;
using Movies.Api.Models;
using Movies.Api.Repositories;

namespace Movies.Api.Services
{
    public class MoviePosterService : IMoviePosterService
    {
        private readonly IMoviePosterRepository _moviePosterRepository;
        private readonly IMoviesRepository _moviesRepository;
        private readonly IMapper _mapper;
        IWebHostEnvironment _environment;
        public MoviePosterService(IMoviePosterRepository moviePosterRepository,
        IMapper mapper,
        IMoviesRepository moviesRepository,
        IWebHostEnvironment environment)
        {
            _moviePosterRepository = moviePosterRepository;
            _mapper = mapper;
            _moviesRepository = moviesRepository;
            _environment = environment;
        }

        public async Task<MoviePosterDto?> GetMoviePosterAsync(int movieId)
        {
            var poster = await _moviePosterRepository.GetMoviesPosterAsync(movieId);
            return _mapper.Map<MoviePosterDto>(poster);
        }

        public async Task<MoviePosterDto> UploadMoviePosterAsync(int movieId, MoviePosterUploadDto moviePosterUploadDto)
        {
            var movie = await _moviesRepository.GetByIdAsync(movieId);
            if (movie == null)
            {
                throw new KeyNotFoundException("The movie was not found.");
            }

            var existingPoster = await _moviePosterRepository.GetMoviesPosterAsync(movieId);
            if (existingPoster != null)
            {
                throw new InvalidOperationException("This movie already has a poster.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(moviePosterUploadDto.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Only JPG, JPEG, PNG, and WEBP images are allowed.");
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (moviePosterUploadDto.Image.Length > maxFileSize)
            {
                throw new ArgumentException("The image cannot be larger than 5 MB.");
            }

            var uploadDirectory = Path.Combine(_environment.ContentRootPath, "uploads", "posters");
            Directory.CreateDirectory(uploadDirectory);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.CreateNew);
            await moviePosterUploadDto.Image.CopyToAsync(fileStream);

            var poster = new MoviePoster
            {
                MovieId = movieId,
                ImageUrl = $"/uploads/posters/{fileName}",
                AltText = moviePosterUploadDto.AltText
            };

            _moviePosterRepository.Create(poster);
            var saved = await _moviePosterRepository.SaveChangesAsync();

            if (!saved)
            {
                File.Delete(filePath);
                throw new InvalidOperationException(
                    "The movie poster could not be saved.");
            }

            return _mapper.Map<MoviePosterDto>(poster);
        }

        public async Task<bool> DeletePosterAsync(int posterId)
        {
            var poster = await _moviePosterRepository.GetByIdAsync(posterId);
            if (poster == null)
            {
                return false;
            }

            DeletePosterFile(poster.ImageUrl);
            _moviePosterRepository.Delete(poster);
            return await _moviePosterRepository.SaveChangesAsync();
        }

        private void DeletePosterFile(string imageUrl)
        {
            var relativePath = imageUrl.TrimStart('/', '\\')
                .Replace('/', Path.DirectorySeparatorChar);

            var filePath = Path.Combine(_environment.ContentRootPath, relativePath);

            var filePathExists = File.Exists(filePath);

            if (filePathExists)
            {
                File.Delete(filePath);
            }
        }
    }
}