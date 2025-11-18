using System.Reactive.Linq;
using Sail.Api.V1;
using Sail.Compass.Observers;
using Sail.Core.Certificates;
using ObserverEventType = Sail.Compass.Observers.EventType;

namespace Sail.Compass.CertificateManager;

internal static class CertificateStreamBuilder
{
    public static IObservable<IReadOnlyList<CertificateConfig>> BuildCertificateStream(
        ResourceObserver<Certificate> certificateObserver)
    {
        return certificateObserver
            .GetObservable(watch: true)
            .Scan(
                seed: new Dictionary<string, Certificate>(StringComparer.OrdinalIgnoreCase),
                accumulator: (certificates, @event) =>
                {
                    var key = @event.Value.CertificateId;
                    var newCertificates = new Dictionary<string, Certificate>(certificates, certificates.Comparer);

                    switch (@event.EventType)
                    {
                        case ObserverEventType.List:
                        case ObserverEventType.Created:
                        case ObserverEventType.Updated:
                            newCertificates[key] = @event.Value;
                            break;
                        case ObserverEventType.Deleted:
                            newCertificates.Remove(key);
                            break;
                    }

                    return newCertificates;
                })
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Select(certificates => ConvertToCertificateConfigs(certificates.Values))
            .StartWith(Array.Empty<CertificateConfig>());
    }

    private static IReadOnlyList<CertificateConfig> ConvertToCertificateConfigs(
        IEnumerable<Certificate> certificates)
    {
        var configs = new List<CertificateConfig>();

        foreach (var certificate in certificates)
        {
            if (string.IsNullOrEmpty(certificate.Value) || string.IsNullOrEmpty(certificate.Key))
            {
                continue;
            }

            if (certificate.Hosts != null && certificate.Hosts.Count > 0)
            {
                foreach (var host in certificate.Hosts)
                {
                    configs.Add(new CertificateConfig
                    {
                        HostName = host,
                        Cert = certificate.Value,
                        Key = certificate.Key
                    });
                }
            }
        }

        return configs;
    }
}

