using ErrorOr;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Models.ServiceDiscoveries;

namespace Sail.Services;

public class ServiceDiscoveryService(IServiceDiscoveryStore store)
{
    public async Task<IEnumerable<ServiceDiscoveryResponse>> ListAsync(string? keywords, CancellationToken cancellationToken)
    {
        var serviceDiscoveries = await store.ListAsync(keywords, cancellationToken);
        return serviceDiscoveries.Select(MapToResponse);
    }

    public async Task<ServiceDiscoveryResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var serviceDiscovery = await store.GetAsync(id, cancellationToken);
        if (serviceDiscovery == null)
        {
            throw new InvalidOperationException($"ServiceDiscovery with id {id} not found");
        }
        return MapToResponse(serviceDiscovery);
    }

    public async Task<ErrorOr<Created>> CreateAsync(ServiceDiscoveryRequest request, CancellationToken cancellationToken)
    {
        var serviceDiscovery = new ServiceDiscovery
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Enabled = request.Enabled,
            Consul = request.Consul != null ? new Consul
            {
                Address = request.Consul.Address,
                Token = request.Consul.Token,
                Datacenter = request.Consul.Datacenter,
                RefreshIntervalSeconds = request.Consul.RefreshIntervalSeconds
            } : null,
            Dns = request.Dns != null ? new Dns
            {
                ServerAddress = request.Dns.ServerAddress,
                RefreshIntervalSeconds = request.Dns.RefreshIntervalSeconds,
                Port = request.Dns.Port
            } : null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await store.CreateAsync(serviceDiscovery, cancellationToken);
        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, ServiceDiscoveryRequest request, CancellationToken cancellationToken)
    {
        var existing = await store.GetAsync(id, cancellationToken);
        if (existing == null)
        {
            return Error.NotFound(description: $"ServiceDiscovery with id {id} not found");
        }

        var serviceDiscovery = new ServiceDiscovery
        {
            Id = id,
            Name = request.Name,
            Type = request.Type,
            Enabled = request.Enabled,
            Consul = request.Consul != null ? new Consul
            {
                Address = request.Consul.Address,
                Token = request.Consul.Token,
                Datacenter = request.Consul.Datacenter,
                RefreshIntervalSeconds = request.Consul.RefreshIntervalSeconds
            } : null,
            Dns = request.Dns != null ? new Dns
            {
                ServerAddress = request.Dns.ServerAddress,
                RefreshIntervalSeconds = request.Dns.RefreshIntervalSeconds,
                Port = request.Dns.Port
            } : null,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await store.UpdateAsync(serviceDiscovery, cancellationToken);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await store.GetAsync(id, cancellationToken);
        if (existing == null)
        {
            return Error.NotFound(description: $"ServiceDiscovery with id {id} not found");
        }

        await store.DeleteAsync(id, cancellationToken);
        return Result.Deleted;
    }

    private static ServiceDiscoveryResponse MapToResponse(ServiceDiscovery serviceDiscovery)
    {
        return new ServiceDiscoveryResponse
        {
            Id = serviceDiscovery.Id,
            Name = serviceDiscovery.Name,
            Type = serviceDiscovery.Type,
            Enabled = serviceDiscovery.Enabled,
            Consul = serviceDiscovery.Consul != null ? new ConsulResponse
            {
                Address = serviceDiscovery.Consul.Address,
                Token = serviceDiscovery.Consul.Token,
                Datacenter = serviceDiscovery.Consul.Datacenter,
                RefreshIntervalSeconds = serviceDiscovery.Consul.RefreshIntervalSeconds
            } : null,
            Dns = serviceDiscovery.Dns != null ? new DnsResponse
            {
                ServerAddress = serviceDiscovery.Dns.ServerAddress,
                RefreshIntervalSeconds = serviceDiscovery.Dns.RefreshIntervalSeconds,
                Port = serviceDiscovery.Dns.Port
            } : null,
            CreatedAt = serviceDiscovery.CreatedAt,
            UpdatedAt = serviceDiscovery.UpdatedAt
        };
    }
}

