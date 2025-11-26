using Microsoft.EntityFrameworkCore;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Database.MongoDB.Extensions;

namespace Sail.Database.MongoDB.Stores;

public class MiddlewareStore(IContext context) : IMiddlewareStore
{
    public async Task<List<Middleware>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await context.Middlewares.ToListAsync(cancellationToken);
    }

    public async Task<Middleware?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Middlewares
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task CreateAsync(Middleware middleware, CancellationToken cancellationToken = default)
    {
        context.Middlewares.Add(middleware);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Middleware middleware, CancellationToken cancellationToken = default)
    {
        context.Middlewares.Update(middleware);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var middleware = await context.Middlewares.FindAsync([id], cancellationToken);
        if (middleware != null)
        {
            context.Middlewares.Remove(middleware);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public IAsyncEnumerable<ChangeStreamEvent<Middleware>> WatchAsync(CancellationToken cancellationToken = default)
    {
        return context.Middlewares.WatchAsync( cancellationToken);
    }
}