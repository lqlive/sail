namespace Sail.Models.Certificates;

public record CertificateRequest(string Cert, string Key, List<SNIRequest>? SNIs = null);