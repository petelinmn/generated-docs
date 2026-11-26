---
title: "Case studies"
lang: en
track: system-design
kind: module
doc: overview
slug: 17-design-case-studies
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Case studies

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Three classic problems in interview format: a URL shortener, a news feed and a notification service. Here everything from the earlier modules comes together into one line of reasoning.

## Key ideas

- You start with requirements and numbers, not technologies.
- A solution is explained through trade-offs.
- A short code is a number in another base or a random string with a check.
- A feed: fan-out on write versus assembly on read.
- Notifications: deduplication, limits, retries, dead letter.
- Bottlenecks and failures are discussed explicitly.
- The simple variant comes first.

## Minimal example

```csharp
Console.WriteLine(Base62.Encode(62)); // "10"
```

## When to use and when not

- ✅ Interview preparation and reviewing your own project.
- ✅ Discussing a design in a team along one structure.
- ❌ Copying someone's solution without your own requirements.
- ❌ Memorizing diagrams instead of reasons.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the walkthrough plan, a URL shortener |
| [2. Advanced](advanced.en.md) | identifiers, a feed, notifications |
| [3. Expert](expert.en.md) | scale, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | base62, an id generator, a feed, notifications |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [design process](../01-design-process/overview.en.md), [caching](../03-caching/overview.en.md), [partitioning](../06-replication-and-partitioning/overview.en.md), [messaging](../08-messaging-and-events/overview.en.md), [reliability](../09-reliability-patterns/overview.en.md)
- A neighbouring module: [multi-region](../16-multi-region-and-dr/overview.en.md)
- In other tracks: [rate limiting](../../../csharp/recipes/rate-limiting/overview.en.md), [background jobs](../../../csharp/recipes/background-jobs/overview.en.md)
