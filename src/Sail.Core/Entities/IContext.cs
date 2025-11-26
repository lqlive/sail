using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Sail.Core.Entities;

public interface IContext
{
    DatabaseFacade Database { get; }
    public DbSet<Cluster> Clusters { get; init; }
    public DbSet<Route> Routes { get; init; }
    public DbSet<Middleware> Middlewares { get; init; }
    public DbSet<Certificate> Certificates { get; init; }
    public DbSet<AuthenticationPolicy> AuthenticationPolicies { get; init; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
