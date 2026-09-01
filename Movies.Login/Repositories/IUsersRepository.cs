using Movies.Login.Models;

namespace Movies.Login.Repositories
{
    public interface IUsersRepository
    {
        void CreateUser(User user);
        Task<List<User>> GetAllUsers();
        Task<User?> GetUserById(int id);
        Task<User?> GetUserByUsername(string username);
        Task<User?> GetUserByEmailOrName(string email, string username);
        void CreateRefreshToken(RefreshToken refreshToken);
        Task<bool> SaveChanges();
        Task<User?> GetUserByRefreshToken(string refreshToken);
    }
}
