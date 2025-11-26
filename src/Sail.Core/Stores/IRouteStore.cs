using Sail.Core.Entities;

namespace Sail.Core.Stores;

public interface IRouteStore
{
    Task<List<Route>> GetAsync(CancellationToken cancellationToken = default);
    Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Route> CreateAsync(Route route, CancellationToken cancellationToken = default);
    Task<Route> UpdateAsync(Route route, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ChangeStreamEvent<Route>> WatchAsync(CancellationToken cancellationToken = default);
}