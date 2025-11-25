namespace Sail.Core.Entities;

public class Certificate
{
    public Guid Id { get; set; }
    public string? Cert { get; set; }
    public string? Key { get; set; }
    public List<SNI>? SNIs { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}