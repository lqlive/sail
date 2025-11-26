using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using MongoDB.EntityFrameworkCore.Metadata;
using MongoDB.EntityFrameworkCore.Storage;

namespace Sail.Database.MongoDB;

public class SailMongoDatabaseCreator : IMongoDatabaseCreator
{
    private readonly IMongoClientWrapper _clientWrapper;
    private readonly IDesignTimeModel _designTimeModel;
    private readonly IUpdateAdapterFactory _updateAdapterFactory;
    private readonly IDatabase _database;
    private readonly MongoDatabaseCreator _baseDatabaseCreator;

    public SailMongoDatabaseCreator(
        IMongoClientWrapper clientWrapper,
        IDesignTimeModel designTimeModel,
        IUpdateAdapterFactory updateAdapterFactory,
        IDatabase database,
        IDiagnosticsLogger<DbLoggerCategory.Database> logger)
    {
        _clientWrapper = clientWrapper;
        _designTimeModel = designTimeModel;
        _updateAdapterFactory = updateAdapterFactory;
        _database = database;
        _baseDatabaseCreator = new MongoDatabaseCreator(clientWrapper, designTimeModel, updateAdapterFactory, database, logger);
    }

    public bool CanConnect() => _baseDatabaseCreator.CanConnect();

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) 
        => _baseDatabaseCreator.CanConnectAsync(cancellationToken);

    public void CreateIndex(IIndex index) 
        => _baseDatabaseCreator.CreateIndex(index);

    public Task CreateIndexAsync(IIndex index, CancellationToken cancellationToken = default) 
        => _baseDatabaseCreator.CreateIndexAsync(index, cancellationToken);

    public void CreateMissingIndexes() 
        => _baseDatabaseCreator.CreateMissingIndexes();

    public Task CreateMissingIndexesAsync(CancellationToken cancellationToken = default) 
        => _baseDatabaseCreator.CreateMissingIndexesAsync(cancellationToken);

    public void CreateMissingVectorIndexes() 
        => _baseDatabaseCreator.CreateMissingVectorIndexes();

    public Task CreateMissingVectorIndexesAsync(CancellationToken cancellationToken = default) 
        => _baseDatabaseCreator.CreateMissingVectorIndexesAsync(cancellationToken);

    public bool DatabaseExists() 
        => _baseDatabaseCreator.DatabaseExists();

    public Task<bool> DatabaseExistsAsync(CancellationToken cancellationToken = default) 
        => _baseDatabaseCreator.DatabaseExistsAsync(cancellationToken);

    public bool EnsureCreated() 
        => EnsureCreated(new MongoDatabaseCreationOptions());

    public bool EnsureCreated(MongoDatabaseCreationOptions options)
    {
        var existed = DatabaseExists();

        if (options.CreateMissingCollections)
        {
            using var collectionNamesCursor = _clientWrapper.Database.ListCollectionNames();
            var collectionNames = collectionNamesCursor.ToList();

            var collectionOptions = new CreateCollectionOptions
            {
                ChangeStreamPreAndPostImagesOptions = new ChangeStreamPreAndPostImagesOptions
                {
                    Enabled = true
                }
            };

            foreach (var entityType in _designTimeModel.Model.GetEntityTypes().Where(e => e.IsDocumentRoot()))
            {
                var collectionName = entityType.GetCollectionName();
                if (!collectionNames.Contains(collectionName))
                {
                    collectionNames.Add(collectionName);
                    try
                    {
                        _clientWrapper.Database.CreateCollection(collectionName, collectionOptions);
                    }
                    catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
                    {
                    }
                }
            }
        }

        if (!existed)
        {
            SeedFromModel();
        }

        if (options.CreateMissingIndexes)
        {
            CreateMissingIndexes();
        }

        if (options.CreateMissingVectorIndexes)
        {
            CreateMissingVectorIndexes();
        }

        if (options.WaitForVectorIndexes)
        {
            WaitForVectorIndexes(options.IndexCreationTimeout);
        }

        return !existed;
    }

    public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default) 
        => EnsureCreatedAsync(new MongoDatabaseCreationOptions(), cancellationToken);

    public async Task<bool> EnsureCreatedAsync(MongoDatabaseCreationOptions options, CancellationToken cancellationToken = default)
    {
        var existed = await DatabaseExistsAsync(cancellationToken);

        if (options.CreateMissingCollections)
        {
            using var collectionNamesCursor = await _clientWrapper.Database.ListCollectionNamesAsync(null, cancellationToken);
            var collectionNames = await collectionNamesCursor.ToListAsync(cancellationToken);

            var collectionOptions = new CreateCollectionOptions
            {
                ChangeStreamPreAndPostImagesOptions = new ChangeStreamPreAndPostImagesOptions
                {
                    Enabled = true
                }
            };

            foreach (var entityType in _designTimeModel.Model.GetEntityTypes().Where(e => e.IsDocumentRoot()))
            {
                var collectionName = entityType.GetCollectionName();
                if (!collectionNames.Contains(collectionName))
                {
                    collectionNames.Add(collectionName);
                    try
                    {
                        await _clientWrapper.Database.CreateCollectionAsync(collectionName, collectionOptions, cancellationToken);
                    }
                    catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
                    {
                    }
                }
            }
        }

        if (!existed)
        {
            await SeedFromModelAsync(cancellationToken);
        }

        if (options.CreateMissingIndexes)
        {
            await CreateMissingIndexesAsync(cancellationToken);
        }

        if (options.CreateMissingVectorIndexes)
        {
            await CreateMissingVectorIndexesAsync(cancellationToken);
        }

        if (options.WaitForVectorIndexes)
        {
            await WaitForVectorIndexesAsync(options.IndexCreationTimeout, cancellationToken);
        }

        return !existed;
    }

    public bool EnsureDeleted() 
        => _baseDatabaseCreator.EnsureDeleted();

    public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default) 
        => _baseDatabaseCreator.EnsureDeletedAsync(cancellationToken);

    public void WaitForVectorIndexes(TimeSpan? timeout = null) 
        => _baseDatabaseCreator.WaitForVectorIndexes(timeout);

    public Task WaitForVectorIndexesAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default) 
        => _baseDatabaseCreator.WaitForVectorIndexesAsync(timeout, cancellationToken);

    private void SeedFromModel()
        => _database.SaveChanges(AddModelData().GetEntriesToSave());

    private async Task SeedFromModelAsync(CancellationToken cancellationToken = default)
        => await _database.SaveChangesAsync(AddModelData().GetEntriesToSave(), cancellationToken);

    private IUpdateAdapter AddModelData()
    {
        var updateAdapter = _updateAdapterFactory.CreateStandalone();
        foreach (var entityType in _designTimeModel.Model.GetEntityTypes())
        {
            foreach (var targetSeed in entityType.GetSeedData())
            {
                var runtimeEntityType = updateAdapter.Model.FindEntityType(entityType.Name)!;
                var entry = updateAdapter.CreateEntry(targetSeed, runtimeEntityType);
                entry.EntityState = EntityState.Added;
            }
        }

        return updateAdapter;
    }
}
