
namespace Sail.Core.RateLimiter;

public interface IRateLimiterPolicyProvider
{
    RateLimiterPolicyConfig? GetPolicy(string key);
}
