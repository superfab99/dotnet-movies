using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Movies.Login.DTOs;
using Movies.Login.Services;

namespace Movies.Login.Controllers
{
    [ApiController]
    [EnableRateLimiting("loginlimit")]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            var result = await _authService.RegisterAsync(userRegisterDto);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] UserLoginDto userLoginDto)
        {
            var result = await _authService.LoginAsync(userLoginDto);
            return result.Token == string.Empty ? Unauthorized(result) : Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            return string.IsNullOrEmpty(result.Token)
                ? Unauthorized(result)
                : Ok(result);
        }
    }
}