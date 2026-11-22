---
title: "Serilog: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: serilog
part: 2
tags: [serilog, custom-sink, destructuring, performance, async-sink, interview-questions]
versions: ".NET 11, C# 15; Serilog 4.4.0"
status: draft
updated: 2026-11-22
---

# Serilog: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

## Internals

The pipeline: a call `Log.Information(template, args)` → the minimum level check (cheap, before parsing) → template parsing (cached) → creating a `LogEvent` with properties (`LogEventProperty`) → enrichers → filters → all sinks. A property is a `LogEventPropertyValue`: `ScalarValue`, `StructureValue` (with `@`), `SequenceValue`, `DictionaryValue`. Verified: the value of `Count` is a `ScalarValue`, and `{@Order}` is a structure.

A custom sink is an implementation of `ILogEventSink.Emit(LogEvent)` (this is how events were captured for verification in the examples):

```csharp
class DelegateSink(Action<LogEvent> a) : ILogEventSink { public void Emit(LogEvent e) => a(e); }
.WriteTo.Sink(new DelegateSink(captured.Add))
```

Every sink gets the event synchronously on the calling thread; slow sinks (network, DB) are wrapped in an asynchronous wrapper (`Serilog.Sinks.Async`) with a bounded queue.

> ⚠️ Verify: `Serilog.Sinks.Async`, the `Seq`/`Elasticsearch` sinks and their delivery reliability were not run in this material.

## Edges and gotchas

**1. Interpolation breaks structure.** Verified: `Log.Information($"interpolated {user}")` gave a string template without properties.

**2. `{Order}` without `@` is `ToString()`.** Verified: the string `Order { … }` instead of a structure.

**3. Culture in the string form.** Verified: in the `ToString()` record the `decimal` came out as `9,5` (the current culture), in the structure — the number `9.5`. For stable output set a `formatProvider` in the sink or the invariant culture.

**4. `Enrich.FromLogContext()` is mandatory.** Without it `LogContext.PushProperty` adds nothing.

**5. `Override` by prefix.** Verified: `Noisy` applies to `Noisy` and `Noisy.Thing` but not to `Other`.

**6. Destructuring complex graphs.** `{@obj}` walks the object; limit circular references and huge collections (`Destructure.ToMaximumDepth`, `ToMaximumCollectionCount`, `Destructure.ByTransforming` policies).

> ⚠️ Verify: the names and behavior of the destructuring-limiting methods were not run in this material.

**7. Losing the tail.** Without `Log.CloseAndFlush()` buffered sinks (file, network) may not write the last messages.

**8. Property name conflicts.** The property `SourceContext`/`Scope` is reserved for service purposes; choose your own name differently.

**9. Cost.** The level check is cheap but creating the arguments (expensive computations before the call) always happens — guard with `Log.IsEnabled(LogEventLevel.Debug)` or `LoggerMessage` on hot paths.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Serilog | a sink ecosystem, structure, `@` destructuring | an extra dependency, synchronous sinks |
| `Microsoft.Extensions.Logging` without Serilog | built in, the `LoggerMessage` source generator | fewer sinks/enrichment |
| NLog | mature, flexible configuration | a different model |
| OpenTelemetry Logs | one standard for logs/metrics/traces | still evolving |
| A third-party agent (stdout → collector) | the app just writes to stdout | needs infrastructure |

## Interview questions

<details><summary>1. What is structured logging?</summary>

Messages are a template + named values; the values are stored as data and available for search and filtering (verified: `User`, `Count` are separate properties).

</details>

<details><summary>2. How does `{@Order}` differ from `{Order}`?</summary>

`@` — destructuring into a structure with properties; without it `ToString()` is called (verified).

</details>

<details><summary>3. Why not log with an interpolated string?</summary>

The template and properties are lost: a ready string without fields remains (verified).

</details>

<details><summary>4. What is a sink?</summary>

An event receiver: console, file, network. A custom sink is an `ILogEventSink`.

</details>

<details><summary>5. How do you add a property to all messages of a request?</summary>

`LogContext.PushProperty` with `Enrich.FromLogContext()` enabled (verified).

</details>

<details><summary>6. How do you mute a noisy source?</summary>

`MinimumLevel.Override("Namespace", LogEventLevel.Warning)` (verified by prefix).

</details>

<details><summary>7. How do you integrate Serilog with `ILogger<T>`?</summary>

`AddSerilog` (Serilog.Extensions.Logging) or `UseSerilog` in the host; `ILogger` messages arrive as Serilog events with properties (verified).

</details>

<details><summary>8. Why `Log.CloseAndFlush()`?</summary>

To flush sink buffers on shutdown, otherwise the tail of the logs can be lost.

</details>

<details><summary>9. Trick question: the log has `{RequestId}` in one message but not in the next. Why?</summary>

The property is set via `LogContext.PushProperty` in a `using` scope; outside the scope it is absent (verified).

</details>

<details><summary>10. Trick question: a `decimal` in the message prints with a comma but in JSON with a dot. Why?</summary>

The text representation uses the current culture, the structured value is a number (verified: `9,5` against `9.5`). Set the invariant culture/`formatProvider`.

</details>

<details><summary>11. How do you avoid paying for `Debug` logs on a hot path?</summary>

The level filter fires before the event is built, but expensive arguments are computed beforehand — wrap in `IsEnabled` or use `LoggerMessage`.

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [22 · Observability](../../modules/22-observability/overview.en.md)
- [19 · Performance](../../modules/19-performance/overview.en.md) — the cost of logging.
- [Kubernetes](../../../kubernetes/overview.en.md) — container logs and collectors.

## Sources

- [Serilog wiki](https://github.com/serilog/serilog/wiki)
- [Serilog: Developing a sink](https://github.com/serilog/serilog/wiki/Developing-a-sink)
- [Serilog: Structured data](https://github.com/serilog/serilog/wiki/Structured-Data)
