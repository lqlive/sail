using Microsoft.Extensions.DependencyInjection.Extensions;
using Sail.Core;
using Sail.Core.Stores;
using Sail.Database.MongoDB.Stores;

namespace Sail.Database.MongoDB.Management;

public static class SailStorageBuilderExtensions
{
    public static SailApplication AddDatabaseStore(this SailApplication application)
    {
        var services = application.Services;
        
        services.TryAddScoped<SailContext>();
        services.TryAddTransient<IRouteStore, RouteStore>();
        services.TryAddTransient<IClusterStore, ClusterStore>();
        services.TryAddTransient<ICertificateStore, CertificateStore>();
        services.TryAddTransient<IMiddlewareStore, MiddlewareStore>();
        services.TryAddTransient<IAuthenticationPolicyStore, AuthenticationPolicyStore>();
        return application;
    }
}


