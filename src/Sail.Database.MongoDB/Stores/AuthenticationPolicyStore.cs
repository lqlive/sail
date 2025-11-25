using Microsoft.EntityFrameworkCore;
using Sail.Core.Entities;
using Sail.Core.Stores;

namespace Sail.Database.MongoDB.Stores;

public class AuthenticationPolicyStore(IContext context) : IAuthenticationPolicyStore
{
    public async Task<AuthenticationPolicy?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.AuthenticationPolicies
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<AuthenticationPolicy>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await context.AuthenticationPolicies.ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(AuthenticationPolicy policy, CancellationToken cancellationToken = default)
    {
        context.AuthenticationPolicies.Add(policy);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AuthenticationPolicy policy, CancellationToken cancellationToken = default)
    {
        context.AuthenticationPolicies.Update(policy);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var policy = await context.AuthenticationPolicies.FindAsync([id], cancellationToken);
        if (policy != null)
        {
            context.AuthenticationPolicies.Remove(policy);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}

