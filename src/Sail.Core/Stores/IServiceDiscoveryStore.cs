using Sail.Core.Entities;

namespace Sail.Core.Stores;

public interface IServiceDiscoveryStore
{
    Task<IEnumerable<ServiceDiscovery>> ListAsync(string? keywords, CancellationToken cancellationToken = default);
    Task<ServiceDiscovery?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(ServiceDiscovery serviceDiscovery, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceDiscovery serviceDiscovery, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

