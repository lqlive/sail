
namespace Sail.Core.Retry;

public interface IRetryPolicyProvider
{
    RetryPolicyConfig? GetPolicy(string key);
}

