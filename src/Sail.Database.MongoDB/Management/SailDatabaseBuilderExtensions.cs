using Sail.Core;
using Sail.Core.Entities;
using Sail.Core.Stores;
using Sail.Database.MongoDB.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace Sail.Database.MongoDB.Management;

public static class SailDatabaseBuilderExtensions
{
    public static SailApplication AddDatabaseStore(this SailApplication application)
    {
        var services = application.Services;

        services.TryAddScoped<IContext>(provider => provider.GetRequiredService<MongoDBContext>());
     
        services.AddDbContext<MongoDBContext>();

        services.TryAddTransient<IRouteStore, RouteStore>();
        services.TryAddTransient<IClusterStore, ClusterStore>();
        services.TryAddTransient<ICertificateStore, CertificateStore>();
        services.TryAddTransient<IMiddlewareStore, MiddlewareStore>();
        services.TryAddTransient<IAuthenticationPolicyStore, AuthenticationPolicyStore>();
        
        return application;
    }
}


