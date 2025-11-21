using MongoDB.Driver;
using Sail.Core.Entities;
using Sail.Core.Stores;

namespace Sail.Database.MongoDB.Stores;

public class ClusterStore(SailContext context) : IClusterStore
{
    public async Task<List<Cluster>> GetAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cluster>.Filter.Empty;
        var clusters = await context.Clusters.FindAsync(filter, cancellationToken: cancellationToken);
        return await clusters.ToListAsync(cancellationToken: cancellationToken);
    }
}