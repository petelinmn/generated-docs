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

## Related materials

- [Advanced theory](advanced.en.md)
- [Quiz](quiz.en.md) and [exercises](exercises.en.md)

## Sources

- [Polly: retry](https://www.pollydocs.org/strategies/retry.html)
- [Polly: circuit breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)
- [.NET: HTTP resilience](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
