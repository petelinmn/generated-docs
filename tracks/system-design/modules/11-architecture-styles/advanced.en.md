---
title: "Architecture styles: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 11-architecture-styles
part: 2
tags: [system-design, architecture, modular-monolith, microservices, fitness-function, adr]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Architecture styles: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Where to draw boundaries

A boundary is good if a typical change stays inside it. Guides: business capabilities, bounded context, different rates of change and load requirements.

## Coupling

The instability of a module is `I = Ce / (Ca + Ce)`, where `Ca` are incoming dependencies and `Ce` outgoing ones. A module many depend on should change rarely. Break dependency cycles with a contract or an event.

## Checking rules with a test

```csharp
public static IEnumerable<string> Violations(Assembly assembly, string module, string forbidden) =>
    from type in assembly.GetTypes()
    where type.Namespace == module
    from used in SignatureTypes(type)
    where used.Namespace == forbidden
    select $"{type.Name} -> {used.Name}";
```

Such a test (a fitness function) fails when `Orders` starts depending on `Billing`. File: [Ex02_BoundaryRules.cs](code/Examples/Ex02_BoundaryRules.cs). It looks only at signatures, not inside method bodies.

## Gradual migration

Strangler fig: put a facade in front of the old system (for example [YARP](../02-scalability-and-load-balancing/basics.en.md)) and move paths to new implementations one by one. Rewriting everything at once is risky.

## ADR

Record the context, the decision, alternatives and consequences. A revision is a new record superseding the old one.

## Best practices

- Start with modules, extract a service when the need is proven.
- One module — one set of data.
- Version and test contracts.
- Have tracing and automated deployment before splitting.

## Common mistakes

- ❌ A service per table → ✅ per business capability.
- ❌ Long synchronous chains → ✅ fewer hops, events.
- ❌ A shared library with domain logic → ✅ duplicating contracts is cheaper than coupling.
- ❌ Architecture only on a diagram → ✅ checks in CI.

## Comparison with other tracks

- Layers, vertical slices, CQRS inside an application — [C#](../../../csharp/modules/21-architecture/overview.en.md).
- Independent deployment — [Kubernetes](../../../kubernetes/modules/05-deployments/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [Azure: architecture styles](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/)
