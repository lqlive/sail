using Microsoft.EntityFrameworkCore.Infrastructure;
using MongoDB.EntityFrameworkCore.Metadata;
using MongoDB.EntityFrameworkCore.Storage;

namespace Sail.Database.MongoDB.Extensions;

public static class MongoDatabaseFacadeExtensions
{
    public static bool EnsureCreated(
        this DatabaseFacade databaseFacade, 
        IMongoDatabaseCreator databaseCreator)
        => databaseCreator.EnsureCreated();

    public static bool EnsureCreated(
        this DatabaseFacade databaseFacade,
        IMongoDatabaseCreator databaseCreator,
        MongoDatabaseCreationOptions options)
        => databaseCreator.EnsureCreated(options);

    public static Task<bool> EnsureCreatedAsync(
        this DatabaseFacade databaseFacade,
        IMongoDatabaseCreator databaseCreator,
        CancellationToken cancellationToken = default)
        => databaseCreator.EnsureCreatedAsync(cancellationToken);

    public static Task<bool> EnsureCreatedAsync(
        this DatabaseFacade databaseFacade,
        IMongoDatabaseCreator databaseCreator,
        MongoDatabaseCreationOptions options,
        CancellationToken cancellationToken = default)
        => databaseCreator.EnsureCreatedAsync(options, cancellationToken);
}

