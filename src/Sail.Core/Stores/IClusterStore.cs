using Sail.Core.Entities;

namespace Sail.Core.Stores;

public interface IClusterStore
{
    Task<List<Cluster>> GetAsync(CancellationToken cancellationToken = default);
    Task<Cluster?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cluster> CreateAsync(Cluster cluster, CancellationToken cancellationToken = default);
    Task<Cluster> UpdateAsync(Cluster cluster, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}