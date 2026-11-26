---
title: "Case studies: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 17-design-case-studies
part: 3
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Case studies: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** modules 01–09 · **Time:** ~3 h

## Why it matters

Knowing patterns is not enough: you need to choose them for a problem and explain the choice.

## The walkthrough plan

| Step | What to get |
|---|---|
| 1. Requirements | functions, constraints, what is out of scope |
| 2. Estimates | QPS, volume, read-to-write ratio |
| 3. API and data | operations and keys |
| 4. Diagram | components and the request flow |
| 5. Deep dive | the hardest place |
| 6. Failures and evolution | what breaks and what is next |

## A URL shortener

**Requirements:** give a short address for a long one; redirect from the short one; reads far outnumber writes.

**Estimates (assumptions):** 1000 new links per second. Seven base62 characters give 3,521,614,606,208 variants; at that rate they last about 112 years.

```csharp
do
{
    buffer[--i] = Alphabet[(int)(value % 62)];
    value /= 62;
}
while (value > 0);
```

**API:** `POST /links` and `GET /{code}`. **Data:** the key is the code, the value is the address, owner and expiry. This is access by key: both a key-value store and a table with a primary key fit. A [cache](../03-caching/overview.en.md) takes the reads. File: [Ex01_UrlShortener.cs](code/Examples/Ex01_UrlShortener.cs).

## 301 or 302

| Code | Meaning | Consequence |
|---|---|---|
| 301 | moved permanently, heuristically cacheable | fewer requests, but clicks may not arrive |
| 302 | temporary, not heuristically cacheable | clicks are visible, changing the address is easier |

If you need analytics and the ability to change or disable a link, take 302.

## Beginner mistakes

- ❌ Drawing sharding right away → ✅ numbers first.
- ❌ Truncating a hash without thinking of collisions → ✅ check uniqueness.
- ❌ Accepting any address → ✅ check the scheme and guard against abuse.
- ❌ Counting clicks synchronously in the request → ✅ an event into a queue.

## Related materials

- [Advanced theory](advanced.en.md)
- [Design process](../01-design-process/overview.en.md)

## Sources

- [RFC 9110: redirection codes](https://www.rfc-editor.org/rfc/rfc9110)
