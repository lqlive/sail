using Sail.Core.Authentication.JwtBearer;
using Sail.Core.Authentication.OpenIdConnect;
using Sail.Core.Entities;

namespace Sail.Core.Authentication;

public class AuthenticationPolicyConfig
{
    public required string Name { get; init; }
    public required AuthenticationSchemeType Type { get; init; }
    public JwtBearerConfiguration? JwtBearer { get; init; }
    public OpenIdConnectConfiguration? OpenIdConnect { get; init; }
}

