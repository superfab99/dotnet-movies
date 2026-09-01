using Movies.Login.DTOs;

namespace Movies.Login.Services
{
    public interface IAuthService
    {
        Task<SignupResultDto> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<LoginResultDto> LoginAsync(UserLoginDto userLoginDto);
        Task<LoginResultDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}