using Polly;

namespace Sail.Core.Retry;

public record RetryPipelineWrapper(RetryPolicyConfig Config, ResiliencePipeline Pipeline);

