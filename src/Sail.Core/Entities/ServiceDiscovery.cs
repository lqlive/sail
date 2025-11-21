namespace Sail.Core.Entities;

public class ServiceDiscovery
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public ServiceDiscoveryType Type { get; init; }
    public bool Enabled { get; init; }
    
    // Consul specific
    public Consul? Consul { get; init; }
    
    // DNS specific
    public Dns? Dns { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public class Consul
{
    public required string Address { get; init; }
    public string? Token { get; init; }
    public string? Datacenter { get; init; }
    public int RefreshIntervalSeconds { get; init; } = 60;
}

public class Dns
{
    public string? ServerAddress { get; init; }
    public int RefreshIntervalSeconds { get; init; } = 300;
    public int Port { get; init; } = 53;
}

