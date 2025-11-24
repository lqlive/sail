using Sail.Core.Entities;
using Sail.Core.Options;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Sail.Database.MongoDB;

public class SailContext
{
    private readonly IMongoDatabase _database;

    private const string ClusterTableName = "clusters";
    private const string RouteTableName = "routes";
    private const string CertificateTableName = "certificates";
    private const string MiddlewareTableName = "middlewares";
    private const string AuthenticationPolicyTableName = "authenticationPolicies";

    public SailContext(IOptions<DatabaseOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    static SailContext()
    {
        RegisterClassMaps();
    }
    public IMongoCollection<Cluster> Clusters => _database.GetCollection<Cluster>(ClusterTableName);
    public IMongoCollection<Route> Routes => _database.GetCollection<Route>(RouteTableName);
    public IMongoCollection<Certificate> Certificates => _database.GetCollection<Certificate>(CertificateTableName);
    public IMongoCollection<Middleware> Middlewares => _database.GetCollection<Middleware>(MiddlewareTableName);
    public IMongoCollection<AuthenticationPolicy> AuthenticationPolicies => _database.GetCollection<AuthenticationPolicy>(AuthenticationPolicyTableName);

    public async Task InitializeAsync()
    {
        var collectionOptions = new CreateCollectionOptions
        {
            ChangeStreamPreAndPostImagesOptions = new ChangeStreamPreAndPostImagesOptions
            {
                Enabled = true
            }
        };
        await _database.CreateCollectionAsync(ClusterTableName, collectionOptions);
        await _database.CreateCollectionAsync(RouteTableName, collectionOptions);
        await _database.CreateCollectionAsync(CertificateTableName, collectionOptions);
        await _database.CreateCollectionAsync(MiddlewareTableName, collectionOptions);
        await _database.CreateCollectionAsync(AuthenticationPolicyTableName, collectionOptions);
    }
    private static void RegisterClassMaps()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Route)))
        {
            BsonClassMap.RegisterClassMap<Route>(classMap =>
            {
                classMap.AutoMap();
                classMap.MapIdMember(x => x.Id)
                    .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                classMap.MapMember(x => x.ClusterId)
                    .SetSerializer(new NullableSerializer<Guid>(new GuidSerializer(GuidRepresentation.Standard)));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Cluster)))
        {
            BsonClassMap.RegisterClassMap<Cluster>(classMap =>
            {
                classMap.AutoMap();
                classMap.MapIdMember(x => x.Id)
                    .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Certificate)))
        {
            BsonClassMap.RegisterClassMap<Certificate>(classMap =>
            {
                classMap.AutoMap();
                classMap.MapIdMember(x => x.Id)
                    .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Destination)))
        {
            BsonClassMap.RegisterClassMap<Destination>(classMap =>
            {
                classMap.AutoMap();
                classMap.MapIdMember(x => x.Id)
                    .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Middleware)))
        {
            BsonClassMap.RegisterClassMap<Middleware>(classMap =>
            {
                classMap.AutoMap();
                classMap.MapIdMember(x => x.Id)
                    .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(AuthenticationPolicy)))
        {
            BsonClassMap.RegisterClassMap<AuthenticationPolicy>(classMap =>
            {
                classMap.AutoMap();
                classMap.MapIdMember(x => x.Id)
                    .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            });
        }

    }
}