
using Microsoft.EntityFrameworkCore;
using Movies.Api.Data;
using Movies.Api.Models;

namespace Movies.Api.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseModel
    {
        protected readonly MoviesApiDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        public Repository(MoviesApiDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public void Create(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;

            _dbSet.Add(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entityToFind = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
            if (entityToFind == null)
                return false;

            Delete(entityToFind);
            return true;
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entityToFind = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
            if (entityToFind == null)
                return null;

            return entityToFind;
        }

        public async Task<bool> SaveChangesAsync()
        {
            var affectedRows = await _dbContext.SaveChangesAsync();

            return affectedRows > 0;
        }

        public void Update(T entity)
        {
            entity.ModifiedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
    }
}