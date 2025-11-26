using Sail.Core.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Sail.Database.MongoDB;

public class MongoDBContext : AbstractContext<MongoDBContext>
{
    static MongoDBContext()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMongoDB("mongodb://localhost:27017", "sail");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cluster>(entity =>
        {
            entity.ToCollection("clusters");
            entity.HasKey(e => e.Id);
            entity.OwnsMany(e => e.Destinations);
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToCollection("routes");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.ToCollection("certificates");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Middleware>(entity =>
        {
            entity.ToCollection("middlewares");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<AuthenticationPolicy>(entity =>
        {
            entity.ToCollection("authenticationPolicies");
            entity.HasKey(e => e.Id);
        });
    }
}