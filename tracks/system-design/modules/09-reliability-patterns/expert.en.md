---
title: "Reliability patterns: expert"
lang: en
track: system-design
kind: module
doc: expert
slug: 09-reliability-patterns
part: 2
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Reliability patterns: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

Retries amplify load exactly when the dependency is weakened. If each of `n` layers makes `a` attempts, `a^n` calls reach the bottom one.

```csharp
public static int CallsAtTheBottom(int attemptsPerLayer, int layers) =>
    Enumerable.Repeat(attemptsPerLayer, layers).Aggregate(1, (total, n) => total * n);
```

Four attempts at three layers make 64 calls. File: [Ex03_Amplification.cs](code/Examples/Ex03_Amplification.cs). A retry budget limits retries to a share of ordinary requests.

### Adaptive concurrency limit

A fixed limit goes stale when the dependency's capacity changes. An adaptive one raises it little by little while things are fine and cuts it sharply on errors or growing latency (AIMD).

```csharp
_limit = failed || latency > latencyTarget
    ? Math.Max(min, _limit * 0.5)
    : Math.Min(max, _limit + 1 / _limit);
```

With a limit of 20, after 21 good answers it became 21, after one slow one 10. File: [Ex04_AdaptiveLimit.cs](code/Examples/Ex04_AdaptiveLimit.cs). This is only the control rule, not a ready limiter.

### Hedging

If the first attempt has not answered within a set time, a second one is started and the winner is taken.

```csharp
.AddHedging(new HedgingStrategyOptions<string> { MaxHedgedAttempts = 1, Delay = delay })
```

In Polly the default is one extra attempt and a 2 second delay; `Delay = TimeSpan.Zero` starts them all at once. Both attempts reach the dependency, so the technique fits safe operations only. The delay is usually set near a high percentile so that a small share of requests is duplicated. File: [Ex05_Hedging.cs](code/Examples/Ex05_Hedging.cs).

### Deadlines in gRPC

By default a gRPC call has no deadline. The client sets `CallOptions.Deadline` (UTC); on expiry the call gets `DeadlineExceeded`, and on the server `ServerCallContext.CancellationToken` is cancelled — the method keeps running until it returns, so the token has to be passed on. The deadline covers retries too. `EnableCallContextPropagation()` passes it to child calls automatically.

### Chaos engineering

The order: describe the steady state with a measurable metric, hypothesize that it will hold, introduce a realistic failure and try to disprove the hypothesis, limiting the blast radius. Polly has the `Fault`, `Outcome`, `Latency` and `Behavior` strategies.

```csharp
.AddChaosFault(new ChaosFaultStrategyOptions
{
    InjectionRate = rate, // always set it: the default is not zero
    Enabled = enabled,
```

### Degradation and feature flags

A gradual rollout keeps each user on one side and widens the share without reshuffling.

```csharp
uint hash = BinaryPrimitives.ReadUInt32BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes($"{flag}\n{userId}")));
return hash % 100 < percent;
```

Everyone who was in 10% stays at 50%. In an application use `Microsoft.FeatureManagement`: it has percentage, time window and targeting filters. A flag is also an emergency switch for a secondary feature. File: [Ex06_FeatureFlags.cs](code/Examples/Ex06_FeatureFlags.cs).

## Reference

| Polly 8.9.1 setting | Default |
|---|---|
| `MaxRetryAttempts` | 3 |
| `Delay` | 2 s |
| `BackoffType` | `Constant` |
| `UseJitter` | `false` |
| `FailureRatio` | 0.1 |
| `MinimumThroughput` | 100 |
| `SamplingDuration` | 30 s |
| `BreakDuration` | 5 s |
| `Timeout` | 30 s |

| Standard HTTP handler | Value |
|---|---|
| rate limiter | 1000 concurrent, no queue |
| total timeout | 30 s |
| retry | 3, exponential with jitter, from 2 s |
| circuit breaker | 10%, minimum 100, window 30 s, break 5 s |
| attempt timeout | 10 s |

It handles HTTP 500 and above, 408, 429, `HttpRequestException` and `TimeoutRejectedException`. The hedging handler (`AddStandardHedgingHandler`) sends an extra attempt without waiting for the answer and fits only safe operations.

| Way to limit load | What it knows in advance | Adapts |
|---|---|---|
| Fixed limit | a number | no |
| Rate limiter | a rate | no |
| AIMD | a latency target | yes |
| Load shedding by priority | request importance | by state |

| Polly chaos | What it injects |
|---|---|
| `Fault` | an exception |
| `Outcome` | a substituted result |
| `Latency` | a delay |
| `Behavior` | any action |

| Degradation level | Example |
|---|---|
| Stale data | a cache instead of the source |
| Simplified answer | without recommendations |
| Read only | writes temporarily off |
| Rejecting some requests | background and unimportant first |

## Gotchas

### Retry by default also repeats programmer errors

```csharp
int calls = 0;
var p = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions { Delay = TimeSpan.Zero }).Build();
try { p.Execute(() => { calls++; throw new ArgumentNullException(); }); } catch (ArgumentNullException) { }
Console.WriteLine(calls);
```

Result: `4`. By default all exceptions except `OperationCanceledException` are handled.

### A timeout does not interrupt code that ignores the token

```csharp
var to = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(100)).Build();
await to.ExecuteAsync(async ct => await Task.Delay(600));
```

Result: the call takes about 600 ms and completes successfully, with no `TimeoutRejectedException`. Cancellation is cooperative: you need `Task.Delay(600, ct)`.

### A breaker with default settings may never trip

99 failed calls in a row through `AddCircuitBreaker(new CircuitBreakerStrategyOptions())` all reached the dependency: `MinimumThroughput` is 100. For a rarely called service this means there is effectively no protection.

### Chaos is enabled by default

```csharp
var c = new ChaosFaultStrategyOptions();
Console.WriteLine($"{c.InjectionRate} {c.Enabled}");
```

Result: `0.001` and `True` (the decimal separator depends on the culture). By adding a strategy without settings you start injecting failures. Enable it explicitly and only where intended.

### Hedging calls the dependency twice

In the test the first attempt hung, the second answered, and the call counter is 2. For a payment without idempotency that is a double charge.

## Trade-offs and alternatives

| Technique | Helps against | Price |
|---|---|---|
| Retry | random failures | load amplification |
| Hedging | a slow tail | extra requests |
| Circuit breaker | long outages | rejection while open |
| Bulkhead | resource capture | idle slots |
| Queue | peaks | delay |
| Cache as a fallback | unavailability | stale data |

## Interview questions

**1. Why a timeout?**

<details><summary>Answer</summary>

So that waiting does not hold resources forever and spread the failure.

</details>

**2. What is a retry storm?**

<details><summary>Answer</summary>

An avalanche of retries that keeps the dependency from recovering.

</details>

**3. Why jitter?**

<details><summary>Answer</summary>

So that clients do not retry in sync.

</details>

**4. The states of a circuit breaker?**

<details><summary>Answer</summary>

Closed, open, half-open (Polly also has isolated for holding it open manually).

</details>

**5. How does a rate limiter differ from a bulkhead?**

<details><summary>Answer</summary>

The first usually limits frequency, the second concurrent calls.

</details>

**6. 429 or 503?**

<details><summary>Answer</summary>

429 means the client exceeded its limit, 503 means the service cannot serve.

</details>

**7. Where do you retry in a chain of three services?**

<details><summary>Answer</summary>

At one layer, usually the one closest to the failure or to the client, with a budget.

</details>

**8. What is deadline propagation?**

<details><summary>Answer</summary>

Passing the remaining time onwards so that work whose result is no longer needed is not done.

</details>

**9. May a payment be retried after a timeout?**

<details><summary>Answer</summary>

Only with an idempotency key: a timeout does not mean the payment failed.

</details>

**10. Trick question: you added retries and availability went up in tests. All good?**

<details><summary>Answer</summary>

Under real overload the same retries may finish the dependency off. Test failure behavior and watch the share of retries.

</details>

**11. How does AIMD work?**

<details><summary>Answer</summary>

The limit grows additively on successes and shrinks multiplicatively on signs of overload.

</details>

**12. How does hedging differ from retry?**

<details><summary>Answer</summary>

It does not wait for a failure: it duplicates a slow request while the first is still running.

</details>

**13. Does a gRPC call have a default deadline?**

<details><summary>Answer</summary>

No, it is set explicitly.

</details>

**14. What must exist before a chaos experiment?**

<details><summary>Answer</summary>

A measurable steady state, a hypothesis, a bounded blast radius and a way to stop the experiment.

</details>

**15. Trick question: you raised a flag from 10% to 50% with a random choice per request. What is wrong?**

<details><summary>Answer</summary>

A user will flip between variants. A stable bucket per user is needed.

</details>

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [Polly: retry](https://www.pollydocs.org/strategies/retry.html)
- [Polly: circuit breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)
- [.NET: HTTP resilience](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- [Polly: hedging](https://www.pollydocs.org/strategies/hedging.html)
- [Polly: chaos engineering](https://www.pollydocs.org/chaos/)
- [gRPC: deadlines and cancellation](https://learn.microsoft.com/aspnet/core/grpc/deadlines-cancellation)
- [Principles of Chaos Engineering](https://principlesofchaos.org/)
- [Microsoft.FeatureManagement](https://learn.microsoft.com/azure/azure-app-configuration/feature-management-dotnet-reference)
