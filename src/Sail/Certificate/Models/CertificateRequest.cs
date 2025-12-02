namespace Sail.Certificate.Models;

public record CertificateRequest(string Name, string Cert, string Key, List<SNIRequest>? SNIs = null);
