namespace Sail.Certificate.Models;

public record CertificateResponse
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Cert { get; init; }
    public string? Key { get; init; }
    public IEnumerable<SNIResponse>? SNIs { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
