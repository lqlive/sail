using Sail.Core.Entities;
using Sail.Core.Stores;
using MongoDB.Driver;

namespace Sail.Database.MongoDB.Stores;

public class MiddlewareStore(SailContext context) : IMiddlewareStore
{
    public async Task<List<Middleware>> GetAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<Middleware>.Filter.Empty;
        var middlewares = await context.Middlewares.FindAsync(filter, cancellationToken: cancellationToken);
        return await middlewares.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<Middleware?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Middleware>.Filter.Eq(m => m.Id, id);
        var middleware = await context.Middlewares.FindAsync(filter, cancellationToken: cancellationToken);
        return await middleware.FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task CreateAsync(Middleware middleware, CancellationToken cancellationToken = default)
    {
        await context.Middlewares.InsertOneAsync(middleware, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Middleware middleware, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Middleware>.Filter.Eq(m => m.Id, middleware.Id);
        await context.Middlewares.ReplaceOneAsync(filter, middleware, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Middleware>.Filter.Eq(m => m.Id, id);
        await context.Middlewares.DeleteOneAsync(filter, cancellationToken: cancellationToken);
    }
}