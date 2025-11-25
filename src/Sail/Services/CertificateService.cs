using ErrorOr;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Models.Certificates;

namespace Sail.Services;

public class CertificateService(ICertificateStore certificateStore)
{
    public async Task<IEnumerable<CertificateResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        var certificates = await certificateStore.GetAsync(cancellationToken);
        return certificates.Select(MapToCertificate);
    }

    public async Task<ErrorOr<Created>> CreateAsync(CertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var certificate = new Certificate
        {
            Cert = request.Cert,
            Key = request.Key,
            SNIs = request.SNIs?.Select(s => new SNI
            {
                Id = Guid.NewGuid(),
                HostName = s.HostName,
                Name = s.Name,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }).ToList()
        };

        await certificateStore.CreateAsync(certificate, cancellationToken);
        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, CertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var certificate = await certificateStore.GetByIdAsync(id, cancellationToken);
        if (certificate is null)
        {
            return Error.NotFound(description: "Certificate not found");
        }

        certificate.Cert = request.Cert;
        certificate.Key = request.Key;
        certificate.UpdatedAt = DateTimeOffset.UtcNow;

        await certificateStore.UpdateAsync(certificate, cancellationToken);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await certificateStore.DeleteAsync(id, cancellationToken);
        return Result.Deleted;
    }

    public async Task<IEnumerable<SNIResponse>> GetSNIsAsync(Guid certificateId,
        CancellationToken cancellationToken = default)
    {
        var certificate = await certificateStore.GetByIdAsync(certificateId, cancellationToken);
        if (certificate is null)
        {
            return [];
        }

        return certificate.SNIs?.Select(MapToSNI) ?? [];
    }

    public async Task<ErrorOr<Created>> CreateSNIAsync(Guid certificateId, SNIRequest request,
        CancellationToken cancellationToken = default)
    {
        var certificate = await certificateStore.GetByIdAsync(certificateId, cancellationToken);
        if (certificate is null)
        {
            return Error.NotFound(description: "Certificate not found");
        }

        var sni = new SNI
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            HostName = request.HostName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        certificate.SNIs ??= new List<SNI>();
        certificate.SNIs.Add(sni);
        certificate.UpdatedAt = DateTimeOffset.UtcNow;

        await certificateStore.UpdateAsync(certificate, cancellationToken);
        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> UpdateSNIAsync(Guid certificateId, Guid id, SNIRequest request,
        CancellationToken cancellationToken = default)
    {
        var certificate = await certificateStore.GetByIdAsync(certificateId, cancellationToken);
        if (certificate is null)
        {
            return Error.NotFound(description: "Certificate not found");
        }

        var sni = certificate.SNIs?.SingleOrDefault(s => s.Id == id);
        if (sni is null)
        {
            return Error.NotFound(description: "SNI not found");
        }

        sni.Name = request.Name;
        sni.HostName = request.HostName;
        sni.UpdatedAt = DateTimeOffset.UtcNow;
        certificate.UpdatedAt = DateTimeOffset.UtcNow;

        await certificateStore.UpdateAsync(certificate, cancellationToken);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteSNIAsync(Guid certificateId, Guid id,
        CancellationToken cancellationToken = default)
    {
        var certificate = await certificateStore.GetByIdAsync(certificateId, cancellationToken);
        if (certificate is null)
        {
            return Error.NotFound(description: "Certificate not found");
        }

        var sni = certificate.SNIs?.SingleOrDefault(s => s.Id == id);
        if (sni is null)
        {
            return Error.NotFound(description: "SNI not found");
        }

        certificate.SNIs!.Remove(sni);
        certificate.UpdatedAt = DateTimeOffset.UtcNow;

        await certificateStore.UpdateAsync(certificate, cancellationToken);
        return Result.Deleted;
    }

    private CertificateResponse MapToCertificate(Certificate certificate)
    {
        return new CertificateResponse
        {
            Id = certificate.Id,
            Cert = certificate.Cert,
            Key = certificate.Key,
            CreatedAt = certificate.CreatedAt,
            UpdatedAt = certificate.UpdatedAt
        };
    }

    private SNIResponse MapToSNI(SNI sni)
    {
        return new SNIResponse
        {
            Id = sni.Id,
            Name = sni.Name,
            HostName = sni.HostName,
            CreatedAt = sni.CreatedAt,
            UpdatedAt = sni.UpdatedAt
        };
    }
}