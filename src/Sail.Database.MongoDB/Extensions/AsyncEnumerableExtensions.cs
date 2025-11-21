using System.Runtime.CompilerServices;
using MongoDB.Driver;

namespace Sail.Database.MongoDB.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<TDocument> ToAsyncEnumerable<TDocument>(
        this IAsyncCursorSource<TDocument> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false);

        await foreach (var document in cursor.ToAsyncEnumerable(cancellationToken))
        {
            yield return document;
        }
    }

    public static async IAsyncEnumerable<TDocument> ToAsyncEnumerable<TDocument>(
        this IAsyncCursor<TDocument> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await source.MoveNextAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var document in source.Current)
            {
                yield return document;
            }
        }
    }
}