using Microsoft.EntityFrameworkCore;
using Sail.Core.Entities;
using Sail.Core.Stores;

namespace Sail.Database.MongoDB.Stores;

public class RouteStore(IContext context) : IRouteStore
{
    public async Task<List<Route>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await context.Routes.ToListAsync(cancellationToken);
    }

    public async Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Routes
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Route> CreateAsync(Route route, CancellationToken cancellationToken = default)
    {
        context.Routes.Add(route);
        await context.SaveChangesAsync(cancellationToken);
        return route;
    }

    public async Task<Route> UpdateAsync(Route route, CancellationToken cancellationToken = default)
    {
        context.Routes.Update(route);
        await context.SaveChangesAsync(cancellationToken);
        return route;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var route = await context.Routes.FindAsync([id], cancellationToken);
        if (route != null)
        {
            context.Routes.Remove(route);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}