using MongoDB.Driver;
using Sail.Core.Entities;
using Sail.Core.Stores;

namespace Sail.Database.MongoDB.Stores;

public class AuthenticationPolicyStore : IAuthenticationPolicyStore
{
    private readonly MongoDBContext _context;

    public AuthenticationPolicyStore(MongoDBContext context)
    {
        _context = context;
    }

    public async Task<AuthenticationPolicy?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AuthenticationPolicy>.Filter.Eq(x => x.Id, id);
        return await _context.AuthenticationPolicies
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<AuthenticationPolicy>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuthenticationPolicies
            .Find(Builders<AuthenticationPolicy>.Filter.Empty)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(AuthenticationPolicy policy, CancellationToken cancellationToken = default)
    {
        await _context.AuthenticationPolicies.InsertOneAsync(policy, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(AuthenticationPolicy policy, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AuthenticationPolicy>.Filter.Eq(x => x.Id, policy.Id);
        await _context.AuthenticationPolicies.ReplaceOneAsync(filter, policy, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AuthenticationPolicy>.Filter.Eq(x => x.Id, id);
        await _context.AuthenticationPolicies.DeleteOneAsync(filter, cancellationToken);
    }
}

