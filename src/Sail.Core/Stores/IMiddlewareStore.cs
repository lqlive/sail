using Sail.Core.Entities;

namespace Sail.Core.Stores;

public interface IMiddlewareStore
{
    Task<List<Middleware>> GetAsync(CancellationToken cancellationToken = default);
    Task<Middleware?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(Middleware middleware, CancellationToken cancellationToken = default);
    Task UpdateAsync(Middleware middleware, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

