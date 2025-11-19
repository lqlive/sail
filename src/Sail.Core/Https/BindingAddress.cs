namespace Sail.Core.Https;

internal class BindingAddress
{
    public string Scheme { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;
    public int Port { get; private set; }
    public string PathBase { get; private set; } = string.Empty;

    public static BindingAddress Parse(string address)
    {
        var result = new BindingAddress();
        
        var uri = new Uri(address, UriKind.Absolute);
        result.Scheme = uri.Scheme;
        result.Host = uri.Host;
        result.Port = uri.Port;
        result.PathBase = uri.PathAndQuery;

        return result;
    }
}

