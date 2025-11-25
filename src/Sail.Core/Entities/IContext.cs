using Microsoft.EntityFrameworkCore;

namespace Sail.Core.Entities;

public interface IContext
{
    public DbSet<Cluster> Clusters { get; init; }
    public DbSet<Route> Routes { get; init; }
    public DbSet<Middleware> Middlewares { get; init; }
    public DbSet<Certificate> Certificates { get; init; }
    public DbSet<AuthenticationPolicy> AuthenticationPolicies { get; init; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
