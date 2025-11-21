using Sail.Core.Entities;
using Sail.Core.Stores;
using MongoDB.Driver;

namespace Sail.Database.MongoDB.Stores;

public class RouteStore(SailContext context) : IRouteStore
{
    public async Task<List<Route>> GetAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Route>.Filter.Empty;
        var routes = await context.Routes.FindAsync(filter, cancellationToken: cancellationToken);
        return await routes.ToListAsync(cancellationToken: cancellationToken);
    }
}