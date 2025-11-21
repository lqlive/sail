using MongoDB.Bson;
using MongoDB.Driver;
using Sail.Core.Entities;
using Sail.Core.Stores;

namespace Sail.Storage.MongoDB.Stores;

public class ServiceDiscoveryStore(SailContext context) : IServiceDiscoveryStore
{
    public async Task<IEnumerable<ServiceDiscovery>> ListAsync(string? keywords, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ServiceDiscovery>.Filter.Empty;
        
        if (!string.IsNullOrWhiteSpace(keywords))
        {
            var keywordFilter = Builders<ServiceDiscovery>.Filter.Or(
                Builders<ServiceDiscovery>.Filter.Regex(x => x.Name, new BsonRegularExpression(keywords, "i"))
            );
            filter &= keywordFilter;
        }

        return await context.ServiceDiscoveries
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceDiscovery?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ServiceDiscoveries
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task CreateAsync(ServiceDiscovery serviceDiscovery, CancellationToken cancellationToken = default)
    {
        await context.ServiceDiscoveries.InsertOneAsync(serviceDiscovery, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(ServiceDiscovery serviceDiscovery, CancellationToken cancellationToken = default)
    {
        await context.ServiceDiscoveries.ReplaceOneAsync(
            x => x.Id == serviceDiscovery.Id,
            serviceDiscovery,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ServiceDiscoveries.DeleteOneAsync(x => x.Id == id, cancellationToken);
    }
}

