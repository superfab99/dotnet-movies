using Movies.Api.Models;

namespace Movies.Api.Repositories;

public interface IActorRepository : IRepository<Actor>
{
    Task<List<Actor>> GetAllAsync();
}
