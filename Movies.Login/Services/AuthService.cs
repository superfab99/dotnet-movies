using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Movies.Login.DTOs;
using Movies.Login.Models;
using Movies.Login.Repositories;

namespace Movies.Login.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsersRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;

        public AuthService(IUsersRepository userRepository,
        IMapper mapper,
        IPasswordHasher<User> passwordHasher,
        ITokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<LoginResultDto> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _userRepository.GetUserByUsername(userLoginDto.Username);
            if (user == null)
            {
                return new LoginResultDto
                {
                    Token = string.Empty,
                    Message = "Invalid username or password."
                };
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLoginDto.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return new LoginResultDto
                {
                    Token = string.Empty,
                    Message = "Invalid username or password."
                };
            }

            var accessToken = _tokenGenerator.GenerateToken(user);
            var refreshTokenValue = _tokenGenerator.GenerateRefreshToken();
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshTokenValue,
                UserId = user.Id,
                CreatedOnUtc = DateTime.UtcNow,
                ExpiresOnUtc = DateTime.UtcNow.AddDays(7)
            };

            _userRepository.CreateRefreshToken(refreshToken);
            await _userRepository.SaveChanges();

            return new LoginResultDto
            {
                Token = accessToken,
                RefreshToken = refreshTokenValue
            };
        }

        public async Task<SignupResultDto> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            //check for existing user with same email or password
            var existingUser = await _userRepository.GetUserByEmailOrName(userRegisterDto.Email, userRegisterDto.Username);
            if (existingUser != null)
            {
                return new SignupResultDto
                {
                    Status = false,
                    Message = "Username or email already exists"
                };
            }

            var user = _mapper.Map<Models.User>(userRegisterDto);
            user.PasswordHash = _passwordHasher.HashPassword(user, userRegisterDto.Password);
            _userRepository.CreateUser(user);
            var saved = await _userRepository.SaveChanges();
            if (!saved)
            {
                return new SignupResultDto
                {
                    Status = false,
                    Message = "Registration could not be completed."
                };
            }

            return new SignupResultDto
            {
                Status = true,
                Message = "Registration successful"
            };
        }

        public async Task<LoginResultDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await _userRepository.GetUserByRefreshToken(request.RefreshToken);

            if (user == null)
            {
                return new LoginResultDto
                {
                    Message = "Invalid refresh token."
                };
            }

            var existingToken = user.RefreshTokens
                .FirstOrDefault(rt => rt.Token == request.RefreshToken);

            if (existingToken == null || !existingToken.IsActive)
            {
                return new LoginResultDto
                {
                    Message = "Invalid refresh token."
                };
            }

            existingToken.RevokedOnUtc = DateTime.UtcNow;

            var newAccessToken = _tokenGenerator.GenerateToken(user);
            var newRefreshTokenValue = _tokenGenerator.GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newRefreshTokenValue,
                UserId = user.Id,
                CreatedOnUtc = DateTime.UtcNow,
                ExpiresOnUtc = DateTime.UtcNow.AddDays(7)
            });

            await _userRepository.SaveChanges();

            return new LoginResultDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshTokenValue
            };
        }
    }
}
