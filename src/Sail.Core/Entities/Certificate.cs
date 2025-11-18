namespace Sail.Core.Entities;

public class Certificate
{
    public Guid Id { get; set; }
    public string? Cert { get; init; }
    public string? Key { get; init; }
    public List<SNI>? SNIs { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}