using System.Globalization;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sail.Core.Utilities;
using Yarp.ReverseProxy.Model;

namespace Sail.Core.Https;

public class HttpsRedirectionMiddleware
{
    private const int PortNotFound = -1;

    private readonly RequestDelegate _next;
    private readonly Lazy<int> _httpsPort;
    private readonly int _statusCode;
    private readonly IServerAddressesFeature? _serverAddressesFeature;
    private readonly IConfiguration _config;
    private readonly ILogger<HttpsRedirectionMiddleware> _logger;

    public HttpsRedirectionMiddleware(
        RequestDelegate next,
        IOptions<HttpsRedirectionOptions> options,
        IConfiguration config,
        ILoggerFactory loggerFactory)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        var httpsRedirectionOptions = options.Value;
        if (httpsRedirectionOptions.HttpsPort.HasValue)
        {
            _httpsPort = new Lazy<int>(httpsRedirectionOptions.HttpsPort.Value);
        }
        else
        {
            _httpsPort = new Lazy<int>(TryGetHttpsPort);
        }

        _statusCode = httpsRedirectionOptions.RedirectStatusCode;
        _logger = loggerFactory.CreateLogger<HttpsRedirectionMiddleware>();
    }

    public HttpsRedirectionMiddleware(
        RequestDelegate next,
        IOptions<HttpsRedirectionOptions> options,
        IConfiguration config,
        ILoggerFactory loggerFactory,
        IServerAddressesFeature serverAddressesFeature)
        : this(next, options, config, loggerFactory)
    {
        _serverAddressesFeature = serverAddressesFeature ?? throw new ArgumentNullException(nameof(serverAddressesFeature));
    }

    public Task Invoke(HttpContext context)
    {
        if (context.Request.IsHttps)
        {
            return _next(context);
        }

        var reverseProxyFeature = context.Features.Get<IReverseProxyFeature>();

        if (!reverseProxyFeature?.Route.GetMetadata<bool>("HttpsRedirect") ?? false)
        {
            return _next(context);
        }

        var port = _httpsPort.Value;
        if (port == PortNotFound)
        {
            Log.FailedToDetermineHttpsPort(_logger, reverseProxyFeature?.Route.Config.RouteId);
            return _next(context);
        }

        var host = context.Request.Host;
        if (port != 443)
        {
            host = new HostString(host.Host, port);
        }
        else
        {
            host = new HostString(host.Host);
        }

        var request = context.Request;
        var redirectUrl = UriHelper.BuildAbsolute(
            "https",
            host,
            request.PathBase,
            request.Path,
            request.QueryString);

        context.Response.StatusCode = _statusCode;
        context.Response.Headers.Location = redirectUrl;

        Log.RedirectingToHttps(_logger, reverseProxyFeature?.Route.Config.RouteId, redirectUrl);

        return Task.CompletedTask;
    }

    private int TryGetHttpsPort()
    {
        var nullablePort = GetIntConfigValue("HTTPS_PORT") ?? GetIntConfigValue("ANCM_HTTPS_PORT");
        if (nullablePort.HasValue)
        {
            var port = nullablePort.Value;
            Log.HttpsPortLoadedFromConfiguration(_logger, port);
            return port;
        }

        if (_serverAddressesFeature == null)
        {
            Log.ServerAddressesFeatureNotAvailable(_logger);
            return PortNotFound;
        }

        foreach (var address in _serverAddressesFeature.Addresses)
        {
            var bindingAddress = BindingAddress.Parse(address);
            if (bindingAddress.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                if (nullablePort.HasValue && nullablePort != bindingAddress.Port)
                {
                    throw new InvalidOperationException(
                        "Cannot determine the HTTPS port from IServerAddressesFeature, multiple values were found. " +
                        "Set the desired port explicitly on HttpsRedirectionOptions.HttpsPort.");
                }
                else
                {
                    nullablePort = bindingAddress.Port;
                }
            }
        }

        if (nullablePort.HasValue)
        {
            var port = nullablePort.Value;
            Log.HttpsPortDiscoveredFromServerAddresses(_logger, port);
            return port;
        }

        Log.FailedToDetermineHttpsPortGeneral(_logger);
        return PortNotFound;

        int? GetIntConfigValue(string name) =>
            int.TryParse(_config[name], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static class Log
    {
        private static readonly Action<ILogger, string?, Exception?> _failedToDetermineHttpsPort = LoggerMessage.Define<string?>(
            LogLevel.Warning,
            new EventId(1, nameof(FailedToDetermineHttpsPort)),
            "Failed to determine HTTPS port for route {RouteId}. HTTPS redirection disabled.");

        private static readonly Action<ILogger, string?, string, Exception?> _redirectingToHttps = LoggerMessage.Define<string?, string>(
            LogLevel.Information,
            new EventId(2, nameof(RedirectingToHttps)),
            "Redirecting to HTTPS. Route: {RouteId}, URL: {RedirectUrl}");

        private static readonly Action<ILogger, int, Exception?> _httpsPortLoadedFromConfiguration = LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(3, nameof(HttpsPortLoadedFromConfiguration)),
            "HTTPS port {Port} loaded from configuration.");

        private static readonly Action<ILogger, Exception?> _serverAddressesFeatureNotAvailable = LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(4, nameof(ServerAddressesFeatureNotAvailable)),
            "Failed to determine HTTPS port. IServerAddressesFeature is not available.");

        private static readonly Action<ILogger, int, Exception?> _httpsPortDiscoveredFromServerAddresses = LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(5, nameof(HttpsPortDiscoveredFromServerAddresses)),
            "HTTPS port {Port} discovered from server addresses.");

        private static readonly Action<ILogger, Exception?> _failedToDetermineHttpsPortGeneral = LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(6, nameof(FailedToDetermineHttpsPortGeneral)),
            "Failed to determine HTTPS port.");

        public static void FailedToDetermineHttpsPort(ILogger logger, string? routeId)
        {
            _failedToDetermineHttpsPort(logger, routeId, null);
        }

        public static void RedirectingToHttps(ILogger logger, string? routeId, string redirectUrl)
        {
            _redirectingToHttps(logger, routeId, redirectUrl, null);
        }

        public static void HttpsPortLoadedFromConfiguration(ILogger logger, int port)
        {
            _httpsPortLoadedFromConfiguration(logger, port, null);
        }

        public static void ServerAddressesFeatureNotAvailable(ILogger logger)
        {
            _serverAddressesFeatureNotAvailable(logger, null);
        }

        public static void HttpsPortDiscoveredFromServerAddresses(ILogger logger, int port)
        {
            _httpsPortDiscoveredFromServerAddresses(logger, port, null);
        }

        public static void FailedToDetermineHttpsPortGeneral(ILogger logger)
        {
            _failedToDetermineHttpsPortGeneral(logger, null);
        }
    }
}
