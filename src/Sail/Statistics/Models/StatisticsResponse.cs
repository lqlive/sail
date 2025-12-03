namespace Sail.Statistics.Models;

public record ResourceCountResponse
{
    public int Total { get; init; }
    public int Enabled { get; init; }
}

public record RecentItemsResponse
{
    public List<RecentItem> Items { get; init; } = [];
}

public record RecentItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

