// Example: hedging and fault injection with Polly.
// Module: system-design/09-reliability-patterns (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Hedging|FullyQualifiedName~Chaos"
using Polly;
using Polly.Hedging;
using Polly.Simmy;
using Polly.Simmy.Fault;

namespace Reliability.Examples;

public static class Hedged
{
    // Start another attempt if the first one has not answered within the delay.
    public static ResiliencePipeline<string> Pipeline(TimeSpan delay, TimeProvider clock) =>
        new ResiliencePipelineBuilder<string> { TimeProvider = clock }
            .AddHedging(new HedgingStrategyOptions<string> { MaxHedgedAttempts = 1, Delay = delay })
            .Build();
}

public static class Chaos
{
    public static ResiliencePipeline Faults(double rate, bool enabled) =>
        new ResiliencePipelineBuilder()
            .AddChaosFault(new ChaosFaultStrategyOptions
            {
                InjectionRate = rate, // always set it: the default is not zero
                Enabled = enabled,
                FaultGenerator = _ => ValueTask.FromResult<Exception?>(new TransientException()),
            })
            .Build();
}
