---
title: "C# / .NET"
lang: en
track: csharp
kind: track
doc: overview
slug: csharp
tags: [csharp, dotnet]
versions: ".NET 11"
default_level: 6
status: draft
updated: 2026-11-20
---

# C# / .NET

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

C# is a statically typed, object-oriented, general-purpose language from Microsoft. Code is compiled to Intermediate Language (IL) and executed by the .NET runtime, which handles JIT compilation and garbage collection and provides a common class library. .NET is a cross-platform, open-source platform that runs on Windows, Linux and macOS. C# is used for web APIs and backends (ASP.NET Core), desktop and console applications, cloud services and games (Unity).

## Environment

| What | Tool |
|---|---|
| SDK | .NET 11 SDK or newer — `dotnet --version`; modules target `net11.0` (C# 15) |
| Editor | VS Code + C# Dev Kit, Rider or Visual Studio |
| Module code | `code/` — a `.slnx` solution: `Examples/` (class library), `Solutions/`, `Tests/` (xUnit v3), plus a `global.json` that switches `dotnet test` to the Microsoft.Testing.Platform mode |
| Run all module tests | `dotnet test` inside `code/` |
| Run one example | `dotnet test --filter "FullyQualifiedName~<Name>"` |
| Quick experiment | `dotnet run scratch.cs` — a file-based app without a project |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **6** (all levels), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · The .NET platform and dotnet CLI](modules/01-dotnet-platform/overview.en.md) — SDK and runtime, CLR, JIT and AOT, csproj/slnx, file-based apps, NuGet
- [02 · Types and variables](modules/02-types-and-variables/overview.en.md) — value and reference types, conversions, strings, nullable reference types
- [03 · Control flow and methods](modules/03-control-flow-and-methods/overview.en.md) — if/switch, loops, ref/out/in parameters, overloading, local functions
- [04 · OOP: classes and objects](modules/04-oop-basics/overview.en.md) — encapsulation, properties, constructors, static, inheritance, polymorphism
- [05 · Interfaces, structs, records](modules/05-interfaces-structs-records/overview.en.md) — interface vs abstract class, struct, record, object equality
- [06 · Collections and generics](modules/06-collections-and-generics/overview.en.md) — `List<T>`, `Dictionary<TKey,TValue>`, `HashSet<T>`, generics, constraints, variance
- [07 · LINQ](modules/07-linq/overview.en.md) — query and method syntax, deferred execution, `IEnumerable<T>` vs `IQueryable<T>`
- [08 · Exceptions](modules/08-exceptions/overview.en.md) — try/catch/finally, hierarchy, custom exceptions, `when` filters, best practices
- [09 · Delegates, events, lambdas](modules/09-delegates-events-lambdas/overview.en.md) — `Func`/`Action`, closures, events

### Part 2. Advanced topics

- [10 · Async/await](modules/10-async-await/overview.en.md) — `Task`, `ValueTask`, `CancellationToken`, deadlocks, common mistakes
- [11 · Modern C#](modules/11-modern-csharp/overview.en.md) — pattern matching, switch expressions, primary constructors, collection expressions, required/init, extension members
- [12 · Memory and GC](modules/12-memory-and-gc/overview.en.md) — stack and heap, GC generations, `IDisposable` and `using`, finalizers and `SafeHandle`, memory leaks, GC modes and settings
- [12a · Span and Memory](modules/12a-span-and-memory/overview.en.md) — `Span<T>`, `Memory<T>`, `stackalloc`, `ref struct`, `System.IO.Pipelines`
- [13 · DI, configuration, logging](modules/13-di-config-logging/overview.en.md) — Generic Host, DI container and lifetimes, Options pattern, `ILogger`
- [14 · ASP.NET Core: Minimal API](modules/14-aspnet-core-minimal-api/overview.en.md) — routing, middleware, validation, OpenAPI, error handling
- [15 · EF Core](modules/15-ef-core/overview.en.md) — DbContext, LINQ → SQL, change tracking, the N+1 problem, concurrency and transactions, inheritance, migrations
- [16 · Testing: unit tests](modules/16-testing-unit/overview.en.md) — xUnit, AAA, parameterized tests, mocks, coverage
- [17 · Testing: integration tests](modules/17-testing-integration/overview.en.md) — `WebApplicationFactory`, Testcontainers, API and database tests

### Part 3. Expert topics

- [18 · Concurrency](modules/18-concurrency/overview.en.md) — threads and ThreadPool, `lock` and `Interlocked`, concurrent collections, Channels, `Parallel`
- [19 · Performance](modules/19-performance/overview.en.md) — BenchmarkDotNet, allocations, `ArrayPool<T>`, profiling, Native AOT
- [20 · Reflection and source generators](modules/20-reflection-and-source-generators/overview.en.md) — Reflection, attributes, expression trees, Roslyn source generators
- [21 · Application architecture](modules/21-architecture/overview.en.md) — layers, Clean Architecture, vertical slices, CQRS, DDD basics
- [22 · Observability](modules/22-observability/overview.en.md) — structured logs, metrics, tracing, OpenTelemetry, health checks

## Libraries

- [Dapper](libraries/dapper/overview.en.md) — micro-ORM: hand-written SQL, automatic mapping
- [Serilog](libraries/serilog/overview.en.md) — structured logging and sinks
- [Polly](libraries/polly/overview.en.md) — resilience: retry, circuit breaker, timeout
- [FluentValidation](libraries/fluentvalidation/overview.en.md) — model validation
- [Refit](libraries/refit/overview.en.md) — typed HTTP clients from interfaces
- [Mapperly](libraries/mapperly/overview.en.md) — object mapping via a source generator
- [NSubstitute](libraries/nsubstitute/overview.en.md) — mocks and stubs in tests
- [Testcontainers](libraries/testcontainers/overview.en.md) — real databases and services in Docker for tests
- [BenchmarkDotNet](libraries/benchmarkdotnet/overview.en.md) — microbenchmarks
- [Spectre.Console](libraries/spectre-console/overview.en.md) — console apps with tables, progress bars, prompts

## Recipes

- [JWT authentication](recipes/jwt-auth/overview.en.md) — issuing and validating tokens in ASP.NET Core
- [Background jobs](recipes/background-jobs/overview.en.md) — `BackgroundService`, queues, graceful shutdown
- [Resilient HttpClient](recipes/http-client-resilience/overview.en.md) — `IHttpClientFactory`, retries, timeouts
- [Caching](recipes/caching/overview.en.md) — `IMemoryCache`, `HybridCache`, Redis
- [Rate limiting](recipes/rate-limiting/overview.en.md) — limiting request rate to an API
- [File upload](recipes/file-upload/overview.en.md) — upload/download, streaming large files
- [Pagination and filtering](recipes/pagination-filtering/overview.en.md) — offset and keyset pagination in an API
- [Real-time with SignalR](recipes/signalr-realtime/overview.en.md) — notifications to the browser
- [gRPC service](recipes/grpc-service/overview.en.md) — a gRPC service and client

## Projects

- [TODO API](projects/todo-api/overview.en.md) — REST API: Minimal API, EF Core, tests, Docker
- [CLI tool](projects/cli-tool/overview.en.md) — a console app with commands and configuration
- [Worker service](projects/worker-service/overview.en.md) — background queue processing with retries and metrics

## Related tracks

- [Python](../python/overview.en.md) — a second language: handy for comparing approaches (async, typing, testing)
- [Kubernetes](../kubernetes/overview.en.md) — deploying C# applications
- [JavaScript](../javascript/overview.en.md) — `async`/`await` came to JavaScript from C#: compare the implementations
- [TypeScript](../typescript/overview.en.md) — structural typing versus nominal typing
- [LLM](../llm/overview.en.md) — LLM applications on .NET with Microsoft.Extensions.AI

## Sources

- [C# documentation](https://learn.microsoft.com/dotnet/csharp/)
- [.NET documentation](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
