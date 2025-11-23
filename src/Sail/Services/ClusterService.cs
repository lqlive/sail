using ErrorOr;
using Sail.Core.Entities;
using MongoDB.Driver;
using Sail.Models.Clusters;
using Sail.Database.MongoDB;

namespace Sail.Services;
public class ClusterService(SailContext context)
{
    public async Task<ClusterResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cluster>.Filter.Where(x => x.Id == id);
        var routes = await context.Clusters.FindAsync(filter, cancellationToken: cancellationToken);
        var result = await routes.SingleOrDefaultAsync(cancellationToken: cancellationToken);
        return MapToCluster(result);
    }

    public async Task<IEnumerable<ClusterResponse>> ListAsync(string keywords,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cluster>.Filter.Empty;
        var routes = await context.Clusters.FindAsync(filter, cancellationToken: cancellationToken);
        var items = await routes.ToListAsync(cancellationToken: cancellationToken);
        return items.Select(MapToCluster);
    }

    public async Task<ErrorOr<Created>> CreateAsync(ClusterRequest request,
        CancellationToken cancellationToken = default)
    {
        var cluster = CreateClusterFromRequest(request);
        await context.Clusters.InsertOneAsync(cluster, cancellationToken: cancellationToken);
        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, ClusterRequest request,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cluster>.Filter.And(Builders<Cluster>.Filter.Where(x => x.Id == id));

        var update = Builders<Cluster>.Update
            .Set(x => x.Name, request.Name)
            .Set(x => x.ServiceName, request.ServiceName)
            .Set(x => x.ServiceDiscoveryType, request.ServiceDiscoveryType)
            .Set(x => x.LoadBalancingPolicy, request.LoadBalancingPolicy)
            .Set(x => x.Destinations, request.Destinations.Select(CreateDestinationFromRequest).ToList())
            .Set(x => x.HealthCheck, CreateHealthCheckFromRequest(request.HealthCheck))
            .Set(x => x.SessionAffinity, CreateSessionAffinityFromRequest(request.SessionAffinity))
            .Set(x => x.UpdatedAt, DateTimeOffset.UtcNow);

        await context.Clusters.FindOneAndUpdateAsync(filter, update, cancellationToken: cancellationToken);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Cluster>.Filter.And(Builders<Cluster>.Filter.Where(x => x.Id == id));
        await context.Clusters.DeleteOneAsync(filter, cancellationToken);
        return Result.Deleted;
    }

    private Cluster CreateClusterFromRequest(ClusterRequest request)
    {
        var cluster = new Cluster
        {
            Name = request.Name,
            LoadBalancingPolicy = request.LoadBalancingPolicy,
            ServiceName = request.ServiceName,
            ServiceDiscoveryType = request.ServiceDiscoveryType,
            HealthCheck = CreateHealthCheckFromRequest(request.HealthCheck),
            SessionAffinity = CreateSessionAffinityFromRequest(request.SessionAffinity),
            Destinations = request.Destinations.Select(CreateDestinationFromRequest).ToList()
        };
        return cluster;
    }

    private HealthCheck CreateHealthCheckFromRequest(HealthCheckRequest? healthCheck)
    {
        return new HealthCheck
        {
            AvailableDestinationsPolicy = healthCheck?.AvailableDestinationsPolicy,
            Active = new ActiveHealthCheck
            {
                Enabled = healthCheck?.Active?.Enabled,
                Interval = healthCheck?.Active?.Interval,
                Timeout = healthCheck?.Active?.Timeout,
                Policy = healthCheck?.Active?.Policy,
                Path = healthCheck?.Active?.Path,
                Query = healthCheck?.Active?.Query
            },
            Passive = new PassiveHealthCheck
            {
                Enabled = healthCheck?.Passive?.Enabled,
                Policy = healthCheck?.Passive?.Policy,
                ReactivationPeriod = healthCheck?.Passive?.ReactivationPeriod,
            }
        };
    }

    private SessionAffinity CreateSessionAffinityFromRequest(SessionAffinityRequest? sessionAffinity)
    {
        return new SessionAffinity
        {
            Enabled = sessionAffinity?.Enabled,
            Policy = sessionAffinity?.Policy,
            FailurePolicy = sessionAffinity?.FailurePolicy,
            AffinityKeyName = sessionAffinity?.AffinityKeyName,
            Cookie = new SessionAffinityCookie
            {
                Path = sessionAffinity?.Cookie?.Path,
                Domain = sessionAffinity?.Cookie?.Domain,
                HttpOnly = sessionAffinity?.Cookie?.HttpOnly,
                SecurePolicy = sessionAffinity?.Cookie?.SecurePolicy,
                SameSite = sessionAffinity?.Cookie?.SameSite,
                Expiration = sessionAffinity?.Cookie?.Expiration,
                MaxAge = sessionAffinity?.Cookie?.MaxAge,
                IsEssential = sessionAffinity?.Cookie?.IsEssential
            }
        };
    }

    private Destination CreateDestinationFromRequest(DestinationRequest request)
    {
        return new Destination
        {
            Id = Guid.NewGuid(),
            Host = request.Host,
            Address = request.Address,
            Health = request.Health
        };
    }

    private ClusterResponse MapToCluster(Cluster cluster)
    {
        return new ClusterResponse
        {
            Id = cluster.Id,
            Name = cluster.Name,
            ServiceName = cluster.ServiceName,
            ServiceDiscoveryType = cluster.ServiceDiscoveryType,
            LoadBalancingPolicy = cluster.LoadBalancingPolicy,
            HealthCheck = new HealthCheckResponse
            {
                AvailableDestinationsPolicy = cluster.HealthCheck?.AvailableDestinationsPolicy,
                Active = new ActiveHealthCheckResponse
                {
                    Enabled = cluster.HealthCheck?.Active?.Enabled,
                    Interval = cluster.HealthCheck?.Active?.Interval,
                    Timeout = cluster.HealthCheck?.Active?.Timeout,
                    Policy = cluster.HealthCheck?.Active?.Policy,
                    Path = cluster.HealthCheck?.Active?.Path,
                    Query = cluster.HealthCheck?.Active?.Query
                },
                Passive = new PassiveHealthCheckResponse
                {
                    Enabled = cluster.HealthCheck?.Passive?.Enabled,
                    Policy = cluster.HealthCheck?.Passive?.Policy,
                    ReactivationPeriod = cluster.HealthCheck?.Passive?.ReactivationPeriod,
                }
            },
            SessionAffinity = new SessionAffinityResponse
            {
                Enabled = cluster.SessionAffinity?.Enabled,
                Policy = cluster.SessionAffinity?.Policy,
                FailurePolicy = cluster.SessionAffinity?.FailurePolicy,
                AffinityKeyName = cluster.SessionAffinity?.AffinityKeyName,
                Cookie = new SessionAffinityCookieResponse
                {
                    Path = cluster.SessionAffinity?.Cookie?.Path,
                    Domain = cluster.SessionAffinity?.Cookie?.Domain,
                    HttpOnly = cluster.SessionAffinity?.Cookie?.HttpOnly,
                    SecurePolicy = cluster.SessionAffinity?.Cookie?.SecurePolicy,
                    SameSite = cluster.SessionAffinity?.Cookie?.SameSite,
                    Expiration = cluster.SessionAffinity?.Cookie?.Expiration,
                    MaxAge = cluster.SessionAffinity?.Cookie?.MaxAge,
                    IsEssential = cluster.SessionAffinity?.Cookie?.IsEssential
                }
            },
            Destinations = cluster.Destinations?.Select(d => new DestinationResponse
            {
                Id = d.Id,
                Host = d.Host,
                Address = d.Address,
                Health = d.Health
            }),
            CreatedAt = cluster.CreatedAt,
            UpdatedAt = cluster.UpdatedAt
        };
    }
}