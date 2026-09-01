using Microsoft.EntityFrameworkCore;
using Movies.Login.Data;
using Movies.Login.Models;

namespace Movies.Login.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly MoviesLoginDbContext _dbContext;
        public UsersRepository(MoviesLoginDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void CreateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            _dbContext.Users.Add(user);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByEmailOrName(string email, string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(e => e.Email == email || e.Username == username);
        }

        public void CreateRefreshToken(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Add(refreshToken);
        }

        public async Task<bool> SaveChanges()
        {
            var affectedRows = await _dbContext.SaveChangesAsync();
            return affectedRows > 0;
        }

        public async Task<User?> GetUserByRefreshToken(string refreshToken)
        {
            return await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));
        }
    }
}
