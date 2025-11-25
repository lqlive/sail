using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Driver;
using Sail.Api.V1;
using Sail.Core.Stores;
using Sail.Database.MongoDB;
using Sail.Database.MongoDB.Extensions;
using Certificate = Sail.Core.Entities.Certificate;
using CertificateResponse = Sail.Api.V1.Certificate;

namespace Sail.Grpc;

public class CertificateGrpcService(MongoDBContext dbContext, ICertificateStore certificateStore)
    : CertificateService.CertificateServiceBase
{
    public override async Task<ListCertificateResponse> List(Empty request, ServerCallContext context)
    {
        var certifications = await certificateStore.GetAsync(CancellationToken.None);
        var response = MapToCertificateItemsResponse(certifications);
        return response;
    }

    public override async Task Watch(Empty request, IServerStreamWriter<WatchCertificateResponse> responseStream,
        ServerCallContext context)
    {
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.Required,
            FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.Required
        };

        while (!context.CancellationToken.IsCancellationRequested)
        {
            var watch = await dbContext.Certificates.WatchAsync(options);

            await foreach (var changeStreamDocument in watch.ToAsyncEnumerable())
            {
                var document = changeStreamDocument.FullDocument;
                if (changeStreamDocument.OperationType == ChangeStreamOperationType.Delete)
                {
                    document = changeStreamDocument.FullDocumentBeforeChange;
                }

                var eventType = changeStreamDocument.OperationType switch
                {
                    ChangeStreamOperationType.Insert => EventType.Create,
                    ChangeStreamOperationType.Update => EventType.Update,
                    ChangeStreamOperationType.Delete => EventType.Delete,
                    _ => EventType.Unknown
                };
                var certificate = MapToCertificateResponse(document);
                var response = new WatchCertificateResponse
                {
                    Certificate = certificate,
                    EventType = eventType
                };
                await responseStream.WriteAsync(response);
            }
        }
    }

    private static ListCertificateResponse MapToCertificateItemsResponse(List<Certificate> certificates)
    {
        var items = certificates.Select(MapToCertificateResponse);

        var response = new ListCertificateResponse
        {
            Items = { items }
        };
        return response;
    }

    private static CertificateResponse MapToCertificateResponse(Certificate certificate)
    {
        return new CertificateResponse
        {
            CertificateId = certificate.Id.ToString(),
            Key = certificate.Key,
            Value = certificate.Cert,
            Hosts = { certificate.SNIs?.Select(item => item.HostName) ?? [] }
        };
    }
}