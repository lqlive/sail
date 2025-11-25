using Microsoft.EntityFrameworkCore;
using Sail.Core.Entities;
using Sail.Core.Stores;

namespace Sail.Database.MongoDB.Stores;

public class CertificateStore(IContext context) : ICertificateStore
{
    public async Task<List<Certificate>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await context.Certificates.ToListAsync(cancellationToken);
    }

    public async Task<Certificate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Certificates
            .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Certificate> CreateAsync(Certificate certificate, CancellationToken cancellationToken = default)
    {
        context.Certificates.Add(certificate);
        await context.SaveChangesAsync(cancellationToken);
        return certificate;
    }

    public async Task<Certificate> UpdateAsync(Certificate certificate, CancellationToken cancellationToken = default)
    {
        context.Certificates.Update(certificate);
        await context.SaveChangesAsync(cancellationToken);
        return certificate;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var certificate = await context.Certificates.FindAsync([id], cancellationToken);
        if (certificate != null)
        {
            context.Certificates.Remove(certificate);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}