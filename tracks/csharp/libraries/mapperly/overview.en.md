---
title: "Mapperly"
lang: en
track: csharp
kind: library
doc: overview
slug: mapperly
part: 2
levels: [1, 2, 3, 4]
tags: [mapperly, object-mapping, source-generator, dto]
versions: ".NET 11, C# 15; Riok.Mapperly 4.3.1"
status: draft
updated: 2026-11-22
---

# Mapperly

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Mapperly is object mapping through a source generator: you declare a `partial` method and at compile time plain C# assignment code is generated. There is no reflection (compatible with Native AOT), the code is visible and debuggable, and forgotten fields produce warnings (`RMG020`). It supports collections, nested paths, updating objects, `IQueryable` projections and enums (by value — by default).

## Key ideas

- `[Mapper] partial class` + `partial` methods.
- Same-named properties — automatically; `[MapProperty]` for the rest; `[MapperIgnoreTarget]`/`[MapperIgnoreSource]` for ignoring.
- `RMG020` — a warning about an unmapped member.
- `Update(source, target)` — updating an existing object.
- An `IQueryable` projection → a `Select` expression for EF Core.
- enum by value by default — be careful.
- Generated files are emitted with `<Compile Remove=…>`.

## Minimal example

```csharp
[Mapper]
partial class OrderMapper
{
    [MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]
    public partial OrderDto ToDto(Order o);
}
```

## When to use and when not

- ✅ Many DTO ↔ model conversions with simple rules.
- ✅ You need AOT, speed, predictability.
- ✅ You want to see mapping errors at build time.
- ❌ Conversions with rich, context-dependent logic — easier by hand.
- ❌ You need dynamic conventions that change at runtime.
- ❌ One or two trivial DTOs — a constructor is enough.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the first mapper, collections, updating, warnings |
| [2. Advanced](advanced.en.md) | enums, projections, `null`, configuration |
| [3. Expert](expert.en.md) | generator internals, gotchas, alternatives, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp mapperly level 5` |
| 6. Tests | not generated yet — `/learn csharp mapperly level 6` |

## Related materials

- Prerequisites: [04 · OOP basics](../../modules/04-oop-basics/overview.en.md), [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md)
- Nearby: [15 · EF Core](../../modules/15-ef-core/overview.en.md), [FluentValidation](../fluentvalidation/overview.en.md), [Dapper](../dapper/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — Pydantic
