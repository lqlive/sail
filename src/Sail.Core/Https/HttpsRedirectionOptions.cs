using Microsoft.AspNetCore.Http;

namespace Sail.Core.Https;

public class HttpsRedirectionOptions
{
    public int? HttpsPort { get; set; }

    public int RedirectStatusCode { get; set; } = StatusCodes.Status308PermanentRedirect;
}

