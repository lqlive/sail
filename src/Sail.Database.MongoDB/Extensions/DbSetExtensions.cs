using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using MongoDB.EntityFrameworkCore.Storage;

namespace Sail.Database.MongoDB.Extensions;

public static class DbSetExtensions
{
    public static Task<IChangeStreamCursor<ChangeStreamDocument<T>>> WatchAsync<T>(
        this DbSet<T> dbSet,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var context = dbSet.GetService<ICurrentDbContext>().Context;
        var mongoClientWrapper = context.GetService<IMongoClientWrapper>();

        var entityType = context.Model.FindEntityType(typeof(T))!;
        var collectionName = entityType.GetCollectionName();
        var collection = mongoClientWrapper.GetCollection<T>(collectionName);

        return collection.WatchAsync(options, cancellationToken);
    }
}