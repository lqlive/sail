using Microsoft.EntityFrameworkCore;

namespace Sail.Core.Entities;

public abstract class AbstractContext<TContext> : DbContext, IContext where TContext : DbContext
{
    public DbSet<Cluster> Clusters { get; init; }
    public DbSet<Route> Routes { get; init; }
    public DbSet<Middleware> Middlewares { get; init; }
    public DbSet<AuthenticationPolicy> AuthenticationPolicies { get; init; }
    public DbSet<Certificate> Certificates { get; init; }
}