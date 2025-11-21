using MongoDB.Driver;
using Sail.Core.Entities;
using Sail.Core.Stores;

namespace Sail.Database.MongoDB.Stores;

public class CertificateStore(SailContext context) : ICertificateStore
{
    public async Task<List<Certificate>> GetAsync(CancellationToken cancellationToken)
    {
        var filter = Builders<Certificate>.Filter.Empty;
        var certificates = await context.Certificates.FindAsync(filter, cancellationToken: cancellationToken);
        return await certificates.ToListAsync(cancellationToken: cancellationToken);
    }
}