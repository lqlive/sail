using ErrorOr;
using Sail.Core.Entities;
using Sail.Core.Stores;
using CertificateEntity = Sail.Core.Entities.Certificate;
using Sail.Certificate.Models;
using Sail.Certificate.Errors;

namespace Sail.Certificate;

public class CertificateService(ICertificateStore certificateStore)
{
    public async Task<IEnumerable<CertificateResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        var certificates = await certificateStore.GetAsync(cancellationToken);
        return certificates.Select(MapToCertificate);
    }

    public async Task<CertificateResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var certificate = await certificateStore.GetByIdAsync(id, cancellationToken);
        return certificate != null ? MapToCertificate(certificate) : null;
    }

    public async Task<ErrorOr<Created>> CreateAsync(CertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var certificate = new CertificateEntity
        {
            Name = request.Name,
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
            return CertificateErrors.CertificateNotFound;
        }

        certificate.Name = request.Name;
        certificate.Cert = request.Cert;
        certificate.Key = request.Key;
        
        certificate.SNIs = request?.SNIs?.Select(s => new SNI
        {
            Id = Guid.NewGuid(),
            HostName = s.HostName,
            Name = s.Name,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        }).ToList() ?? [];
        
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
            return CertificateErrors.CertificateNotFound;
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
            return CertificateErrors.CertificateNotFound;
        }

        var sni = certificate.SNIs?.SingleOrDefault(s => s.Id == id);
        if (sni is null)
        {
            return CertificateErrors.SNINotFound;
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
            return CertificateErrors.CertificateNotFound;
        }

        var sni = certificate.SNIs?.SingleOrDefault(s => s.Id == id);
        if (sni is null)
        {
            return CertificateErrors.SNINotFound;
        }

        certificate.SNIs!.Remove(sni);
        certificate.UpdatedAt = DateTimeOffset.UtcNow;

        await certificateStore.UpdateAsync(certificate, cancellationToken);
        return Result.Deleted;
    }

    private CertificateResponse MapToCertificate(CertificateEntity certificate)
    {
        return new CertificateResponse
        {
            Id = certificate.Id,
            Name = certificate.Name ?? $"Certificate {certificate.Id.ToString().Substring(0, 8)}",
            Cert = certificate.Cert,
            Key = certificate.Key,
            SNIs = certificate.SNIs?.Select(MapToSNI),
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

