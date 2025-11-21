using Sail.Core.Entities;

namespace Sail.Models.ServiceDiscoveries;

public record ServiceDiscoveryRequest
{
    public required string Name { get; init; }
    public ServiceDiscoveryType Type { get; init; }
    public bool Enabled { get; init; }
    public ConsulRequest? Consul { get; init; }
    public DnsRequest? Dns { get; init; }
}

public record ConsulRequest
{
    public required string Address { get; init; }
    public string? Token { get; init; }
    public string? Datacenter { get; init; }
    public int RefreshIntervalSeconds { get; init; } = 60;
}

public record DnsRequest
{
    public string? ServerAddress { get; init; }
    public int RefreshIntervalSeconds { get; init; } = 300;
    public int Port { get; init; } = 53;
}

