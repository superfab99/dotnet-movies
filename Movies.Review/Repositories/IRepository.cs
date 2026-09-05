using Movies.Review.Models;

namespace Movies.Api.Repositories
{
    public interface IRepository<T> where T : BaseModel
    {
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> DeleteAsync(int id);
        Task<T?> GetByIdAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}