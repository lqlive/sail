using Sail.Core.Entities;

namespace Sail.Models.ServiceDiscoveries;

public record ServiceDiscoveryResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public ServiceDiscoveryType Type { get; init; }
    public bool Enabled { get; init; }
    public ConsulResponse? Consul { get; init; }
    public DnsResponse? Dns { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record ConsulResponse
{
    public required string Address { get; init; }
    public string? Token { get; init; }
    public string? Datacenter { get; init; }
    public int RefreshIntervalSeconds { get; init; }
}

public record DnsResponse
{
    public string? ServerAddress { get; init; }
    public int RefreshIntervalSeconds { get; init; }
    public int Port { get; init; }
}

