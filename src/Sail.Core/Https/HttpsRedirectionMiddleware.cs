using System.Globalization;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        if (reverseProxyFeature?.Route.Config.Metadata?.TryGetValue("HttpsRedirect", out var httpsRedirect) != true
            || httpsRedirect != "true")
        {
            return _next(context);
        }

        var port = _httpsPort.Value;
        if (port == PortNotFound)
        {
            _logger.LogWarning(
                "Failed to determine HTTPS port for route {RouteId}. HTTPS redirection disabled.",
                reverseProxyFeature.Route.Config.RouteId);
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

        _logger.LogInformation(
            "Redirecting to HTTPS. Route: {RouteId}, URL: {RedirectUrl}",
            reverseProxyFeature.Route.Config.RouteId,
            redirectUrl);

        return Task.CompletedTask;
    }

    private int TryGetHttpsPort()
    {
        var nullablePort = GetIntConfigValue("HTTPS_PORT") ?? GetIntConfigValue("ANCM_HTTPS_PORT");
        if (nullablePort.HasValue)
        {
            var port = nullablePort.Value;
            _logger.LogDebug("HTTPS port {Port} loaded from configuration.", port);
            return port;
        }

        if (_serverAddressesFeature == null)
        {
            _logger.LogWarning("Failed to determine HTTPS port. IServerAddressesFeature is not available.");
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
            _logger.LogDebug("HTTPS port {Port} discovered from server addresses.", port);
            return port;
        }

        _logger.LogWarning("Failed to determine HTTPS port.");
        return PortNotFound;

        int? GetIntConfigValue(string name) =>
            int.TryParse(_config[name], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) ? value : null;
    }
}

