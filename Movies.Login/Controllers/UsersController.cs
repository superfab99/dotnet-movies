using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Login.DTOs;
using Movies.Login.Repositories;

namespace Movies.Login.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUsersRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsers();
            var userDtos = users.Select(u => _mapper.Map<UserReadDto>(u));
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            var userDto = _mapper.Map<UserReadDto>(user);
            return Ok(userDto);
        }

        [Authorize(Policy = "MovieWrite")]
        [HttpGet("movie-write-test")]
        public IActionResult MovieWriteTest()
        {
            return Ok("You have movie.write permission.");
        }
    }
}
