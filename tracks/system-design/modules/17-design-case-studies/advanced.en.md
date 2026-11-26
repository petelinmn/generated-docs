---
title: "Case studies: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 17-design-case-studies
part: 3
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Case studies: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Generating identifiers

| Method | Pro | Con |
|---|---|---|
| Database counter | simple | a central point, predictable codes |
| Ranges handed to instances | few round trips | gaps, predictability |
| Time + node + sequence | no coordination | depends on the clock and node uniqueness |
| Random string | unpredictable | uniqueness check |

```csharp
// Refusing is safer than issuing an id that may already exist.
if (ms < _lastMs) throw new InvalidOperationException("The clock moved backwards.");
```

The bit layout: 41 for milliseconds, 10 for the node, 12 for the sequence. The guard against a clock jumping backwards works only within the life of the process.

## A news feed

**Fan-out on write:** a post is put into every follower's inbox — reading is fast. **On read:** the feed is assembled from authors' posts — writing is cheap. A hybrid:

```csharp
if (followers.Count >= popularThreshold)
{
    Bucket(_popular, author).Add(postId);
    return;
}
```

For an author with millions of followers copying a post is expensive, so their posts are mixed in at read time. File: [Ex02_NewsFeed.cs](code/Examples/Ex02_NewsFeed.cs).

## A notification service

```csharp
if (_accepted.Contains(n.Id)) return SendResult.Duplicate;
if (_sentToUser.GetValueOrDefault(n.UserId) >= maxPerUser) return SendResult.RateLimited;
_accepted.Add(n.Id);
```

Then send attempts and a dead letter list. File: [Ex03_Notifications.cs](code/Examples/Ex03_Notifications.cs). This models the order of checks: a system needs a queue, durable storage, delays between retries and a limit per time window.

## Best practices

- Say your assumptions out loud.
- Show the simple solution, then the place where it stops working.
- Describe the failure behavior of every component.
- Name metrics together with the solution.

## Common mistakes

- ❌ One feed strategy for all authors → ✅ a hybrid.
- ❌ Retrying a send without idempotency at the provider → ✅ an idempotency key, otherwise duplicates.
- ❌ Forgetting deletion and privacy → ✅ account for them from the start.
- ❌ A hot key for a popular link → ✅ caching at several levels.

## Comparison with other tracks

- Rate limiting in ASP.NET Core — a [recipe](../../../csharp/recipes/rate-limiting/overview.en.md).
- Queue processing — [background jobs](../../../csharp/recipes/background-jobs/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [RFC 9110: redirection codes](https://www.rfc-editor.org/rfc/rfc9110)
