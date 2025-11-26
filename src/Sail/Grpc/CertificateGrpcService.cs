using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sail.Api.V1;
using Sail.Core.Stores;
using Certificate = Sail.Core.Entities.Certificate;
using CertificateResponse = Sail.Api.V1.Certificate;

namespace Sail.Grpc;

public class CertificateGrpcService(ICertificateStore certificateStore)
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
        await foreach (var changeEvent in certificateStore.WatchAsync(context.CancellationToken))
        {
            var eventType = changeEvent.OperationType switch
            {
                ChangeStreamType.Insert => EventType.Create,
                ChangeStreamType.Update => EventType.Update,
                ChangeStreamType.Delete => EventType.Delete,
                _ => EventType.Unknown
            };

            var certificate = MapToCertificateResponse(changeEvent.Document);
            var response = new WatchCertificateResponse
            {
                Certificate = certificate,
                EventType = eventType
            };
            await responseStream.WriteAsync(response);
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