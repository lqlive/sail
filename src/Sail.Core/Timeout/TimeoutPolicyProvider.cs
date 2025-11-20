using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sail.Core.Timeout;

public class TimeoutPolicyProvider(
    IOptionsMonitorCache<RequestTimeoutOptions> optionsCache,
    ILogger<TimeoutPolicyProvider> logger)
{
    public Task UpdateAsync(IReadOnlyList<TimeoutPolicyConfig> policies, CancellationToken cancellationToken)
    {
        var newOptions = new RequestTimeoutOptions();
        
        foreach (var policy in policies)
        {
            newOptions.Policies[policy.Name] = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(policy.Seconds),
                TimeoutStatusCode = policy.TimeoutStatusCode
            };
        }
        optionsCache.TryRemove(Microsoft.Extensions.Options.Options.DefaultName);
        optionsCache.TryAdd(Microsoft.Extensions.Options.Options.DefaultName, newOptions);

        return Task.CompletedTask;
    }
}
