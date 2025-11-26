using Microsoft.EntityFrameworkCore;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Database.MongoDB.Extensions;

namespace Sail.Database.MongoDB.Stores;

public class ClusterStore(IContext context) : IClusterStore
{
    public async Task<List<Cluster>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await context.Clusters
            .ToListAsync(cancellationToken);
    }

    public async Task<Cluster?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
    
        return await context.Clusters
            .SingleAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cluster> CreateAsync(Cluster cluster, CancellationToken cancellationToken = default)
    {
        context.Clusters.Add(cluster);
        await context.SaveChangesAsync(cancellationToken);
        return cluster;
    }

    public async Task<Cluster> UpdateAsync(Cluster cluster, CancellationToken cancellationToken = default)
    {
        context.Clusters.Update(cluster);
        await context.SaveChangesAsync(cancellationToken);
        return cluster;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cluster = await context.Clusters.FindAsync([id], cancellationToken);
        if (cluster != null)
        {
            context.Clusters.Remove(cluster);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public IAsyncEnumerable<ChangeStreamEvent<Cluster>> WatchAsync(CancellationToken cancellationToken = default)
    {
        return context.Clusters.WatchAsync(cancellationToken);
    }
}