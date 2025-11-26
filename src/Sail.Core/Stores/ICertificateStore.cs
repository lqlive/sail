using Sail.Core.Entities;

namespace Sail.Core.Stores;

public interface ICertificateStore
{
    Task<List<Certificate>> GetAsync(CancellationToken cancellationToken = default);
    Task<Certificate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Certificate> CreateAsync(Certificate certificate, CancellationToken cancellationToken = default);
    Task<Certificate> UpdateAsync(Certificate certificate, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ChangeStreamEvent<Certificate>> WatchAsync(CancellationToken cancellationToken = default);
}