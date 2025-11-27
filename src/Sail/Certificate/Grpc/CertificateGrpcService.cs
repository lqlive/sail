using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sail.Api.V1;
using Sail.Core.Stores;
using CertificateEntity = Sail.Core.Entities.Certificate;
using CertificateResponse = Sail.Api.V1.Certificate;
using GrpcCertificateService = Sail.Api.V1.CertificateService;

namespace Sail.Certificate.Grpc;

public class CertificateGrpcService(ICertificateStore certificateStore)
    : GrpcCertificateService.CertificateServiceBase
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

    private static ListCertificateResponse MapToCertificateItemsResponse(List<CertificateEntity> certificates)
    {
        var items = certificates.Select(MapToCertificateResponse);

        var response = new ListCertificateResponse
        {
            Items = { items }
        };
        return response;
    }

    private static CertificateResponse MapToCertificateResponse(CertificateEntity certificate)
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

