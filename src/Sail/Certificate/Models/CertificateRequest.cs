namespace Sail.Certificate.Models;

public record CertificateRequest(string Cert, string Key, List<SNIRequest>? SNIs = null);
