using Movies.Login.Models;

namespace Movies.Login.Services
{
    public interface ITokenGenerator
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}