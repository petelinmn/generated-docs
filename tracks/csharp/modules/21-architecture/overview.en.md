---
title: "Application architecture"
lang: en
track: csharp
kind: module
doc: overview
slug: 21-architecture
part: 3
levels: [1, 2, 3, 4]
tags: [architecture, clean-architecture, ddd, cqrs, vertical-slices, hexagonal-architecture, architecture-tests]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Application architecture

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

An application's architecture is a way to divide code into parts and agree on who depends on whom, so that changes are cheap and business logic can be checked without a database or network. The foundation is the dependency rule: the domain sits in the center and knows nothing of the outside world, interfaces (ports) belong to the core, while databases, the web and mail plug in through adapters. On top of this come a rich domain model, CQRS, vertical slices and tactical DDD; large systems add bounded contexts and reliable events.

## Key ideas

- **Layers:** presentation → application → domain ← infrastructure; dependencies point to the domain.
- **A rich model:** an aggregate protects invariants; value objects are a `record`/`readonly record struct`.
- **Ports and adapters:** the interface belongs to the consumer, the implementation to an outer layer; the composition root ties them together.
- **CQRS:** commands change state, queries read; a dispatcher and handlers.
- **Vertical slices:** code by feature, not by technical role.
- **DDD:** ubiquitous language, entity, value, aggregate, repository, domain events, bounded context.
- **Protecting boundaries:** architecture tests on references between assemblies.
- **Complexity in proportion to the task:** CQRS/DDD/microservices are not for every CRUD.

## Minimal example

```csharp
var services = new ServiceCollection().AddApplication().AddInfrastructure();
using var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<Mediator>();

var id = await mediator.SendAsync(new PlaceOrderCommand("EUR", [("book", 2, 12.50m), ("pen", 3, 1.20m)]));
var summary = await mediator.SendAsync(new GetOrderSummaryQuery(id));
Console.WriteLine($"{summary!.Lines} lines, total {summary.Total.ToString(System.Globalization.CultureInfo.InvariantCulture)} {summary.Currency}");
// 2 lines, total 28.60 EUR
```

## When to use and when not

- ✅ Long-lived systems with complex business rules and tests.
- ✅ Teams that need clear boundaries and independent changes.
- ✅ Architecture tests and a composition root even in small projects.
- ❌ CQRS/DDD/layers for a CRUD form — cost without benefit.
- ❌ A universal `IRepository<T>` with `IQueryable` on top of EF Core.
- ❌ Microservices before a real need appears.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | why architecture, layers and the dependency rule, a rich model, value objects, the composition root |
| [2. Advanced](advanced.en.md) | Clean/Hexagonal, ports and adapters, CQRS and the mediator, slices, DDD basics, domain events, architecture tests |
| [3. Expert](expert.en.md) | bounded context and ACL, Outbox and consistency, monolith versus microservices, a repository and EF Core, gotchas, 12 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 21-architecture level 5` |
| 6. Tests | not generated yet — `/learn csharp 21-architecture level 6` |

## Related materials

- Prerequisites: [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md), [16 · Unit tests](../16-testing-unit/overview.en.md)
- Data: [15 · EF Core](../15-ef-core/overview.en.md)
- Next: [22 · Observability](../22-observability/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — application architecture
- Recipes and libraries: [FluentValidation](../../libraries/fluentvalidation/overview.en.md), [Polly](../../libraries/polly/overview.en.md), [Testcontainers](../../libraries/testcontainers/overview.en.md), [Background jobs](../../recipes/background-jobs/overview.en.md), [Caching](../../recipes/caching/overview.en.md), [File upload](../../recipes/file-upload/overview.en.md), [gRPC service](../../recipes/grpc-service/overview.en.md), [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md), [JWT authentication](../../recipes/jwt-auth/overview.en.md), [Pagination and filtering](../../recipes/pagination-filtering/overview.en.md), [Rate limiting](../../recipes/rate-limiting/overview.en.md), [Real-time with SignalR](../../recipes/signalr-realtime/overview.en.md)
- [System design: design process](../../../system-design/modules/01-design-process/overview.en.md)
- [System design: architecture styles](../../../system-design/modules/11-architecture-styles/overview.en.md)
