namespace Sail.Core.Retry;

public interface IRetryPolicyProvider
{
    RetryPipelineWrapper? GetPolicy(string key);
}

