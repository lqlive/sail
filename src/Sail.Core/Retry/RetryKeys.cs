using Polly;

namespace Sail.Core.Retry;

internal static class RetryKeys
{
    public static readonly ResiliencePropertyKey<Action> OnRetryCallback = new("Sail.OnRetryCallback");
}

