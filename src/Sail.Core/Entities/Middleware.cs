namespace Sail.Core.Entities;

public enum MiddlewareType
{
    Cors,
    RateLimiter,
    Timeout,
    Retry
}

public class Middleware
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MiddlewareType Type { get; set; }
    public bool Enabled { get; set; } = true;
    public Cors? Cors { get; set; }
    public RateLimiter? RateLimiter { get; set; }
    public Timeout? Timeout { get; set; }
    public Retry? Retry { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Cors
{
    public string Name { get; set; } = string.Empty;
    public List<string>? AllowOrigins { get; set; }
    public List<string>? AllowMethods { get; set; }
    public List<string>? AllowHeaders { get; set; }
    public List<string>? ExposeHeaders { get; set; }
    public bool AllowCredentials { get; set; }
    public int? MaxAge { get; set; }
}

public class RateLimiter
{
    public string Name { get; set; } = string.Empty;
    public int PermitLimit { get; set; }
    public int Window { get; set; }
    public int QueueLimit { get; set; }
}

public class Timeout
{
    public string Name { get; set; } = string.Empty;
    public int Seconds { get; set; }
    public int? TimeoutStatusCode { get; set; }
}

public class Retry
{
    public string Name { get; set; } = string.Empty;
    public int MaxRetryAttempts { get; set; } = 1;
    public int[] RetryStatusCodes { get; set; } = [500, 502, 503, 504];
    public int RetryDelayMilliseconds { get; set; } = 1000;
    public bool UseExponentialBackoff { get; set; } = false;
}
