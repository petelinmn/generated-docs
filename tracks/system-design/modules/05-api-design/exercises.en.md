---
title: "Exercises: API design"
lang: en
track: system-design
kind: module
doc: exercises
slug: 05-api-design
part: 1
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Exercises: API design

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 May the request be retried

**Task:** from the method and the presence of an idempotency key, decide whether automatic retry is allowed.

**Expected result:**

```text
get -> true, POST -> false, PATCH -> false, POST with key -> true
```

<details><summary>Hint</summary>

Recall the list of idempotent methods.

</details>

<details><summary>Solution</summary>

```csharp
public static bool IsRetrySafe(string method, bool hasIdempotencyKey) =>
    method.ToUpperInvariant() is "GET" or "HEAD" or "OPTIONS" or "TRACE" or "PUT" or "DELETE"
    || hasIdempotencyKey;
```

The key helps only if the server supports it. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Safe cursor parsing

**Task:** parse a cursor without exceptions for any input.

**Expected result:**

```text
"NDI" -> true, 42; "###", "YWJj", null -> false
```

<details><summary>Hint</summary>

Two checks: valid Base64Url and a number inside.

</details>

<details><summary>Solution</summary>

```csharp
id = 0;
if (string.IsNullOrEmpty(cursor) || !Base64Url.IsValid(cursor)) return false;

string text = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(cursor));
return long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out id);
```

Encoding is not protection: a client can substitute any number, permissions are checked separately.

</details>

## 3. 🔴 If-Match evaluation

**Task:** support `*`, a list of tags and a missing resource; weak tags do not match.

**Expected result:**

```text
"*" with "1" -> true; list "1","2" with "2" -> true; weak -> false; no resource -> false
```

<details><summary>Hint</summary>

First the missing resource, then `*`.

</details>

<details><summary>Solution</summary>

```csharp
if (currentETag is null) return false;          // nothing to match
if (header.Trim() == "*") return true;
if (currentETag.StartsWith("W/", StringComparison.Ordinal)) return false;

return header.Split(',', StringSplitOptions.TrimEntries).Contains(currentETag);
```

A simplification: a comma inside a tag is not supported; in production use ready-made header parsing.

</details>
