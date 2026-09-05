using Microsoft.EntityFrameworkCore;
using Movies.Api.Data;
using Movies.Api.Models;

namespace Movies.Api.Repositories;

public class ActorRepository : Repository<Actor>, IActorRepository
{
    public ActorRepository(
        MoviesApiDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<List<Actor>> GetAllAsync()
    {
        return _dbSet
            .AsNoTracking()
            .OrderBy(actor => actor.LastName)
            .ThenBy(actor => actor.FirstName)
            .ToListAsync();
    }
}
