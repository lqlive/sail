using Sail.Core.Entities;

namespace Sail.Core.Stores;

public interface IAuthenticationPolicyStore
{
    Task<AuthenticationPolicy?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<AuthenticationPolicy>> GetAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(AuthenticationPolicy policy, CancellationToken cancellationToken = default);
    Task UpdateAsync(AuthenticationPolicy policy, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

