using MongoDB.Driver;
using Sail.Core.Stores;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

    public static async IAsyncEnumerable<ChangeStreamEvent<T>> WatchAsync<T>(
        this DbSet<T> dbSet,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class
    {
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
            FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.Required
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            var watch = await dbSet.WatchAsync(options, cancellationToken);

            await foreach (var changeStreamDocument in watch.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                var document = changeStreamDocument.FullDocument;
                if (changeStreamDocument.OperationType == ChangeStreamOperationType.Delete)
                {
                    document = changeStreamDocument.FullDocumentBeforeChange;
                }

                var operationType = changeStreamDocument.OperationType switch
                {
                    ChangeStreamOperationType.Insert => ChangeStreamType.Insert,
                    ChangeStreamOperationType.Update => ChangeStreamType.Update,
                    ChangeStreamOperationType.Delete => ChangeStreamType.Delete,
                    ChangeStreamOperationType.Replace => ChangeStreamType.Replace,
                    ChangeStreamOperationType.Invalidate => ChangeStreamType.Invalidate,
                    _ => ChangeStreamType.Insert
                };

                yield return new ChangeStreamEvent<T>
                {
                    Document = document,
                    OperationType = operationType
                };
            }
        }
    }
}