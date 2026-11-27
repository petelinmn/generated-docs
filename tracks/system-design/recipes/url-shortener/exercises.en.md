---
title: "Exercises: uRL shortener"
lang: en
track: system-design
kind: recipe
doc: exercises
slug: url-shortener
part: 2
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# Exercises: uRL shortener

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are in `code/Solutions/` and covered by tests.

## 1. 🟢 A check character

**Task:** append a character to a code so that a typo is detected without a database lookup.

<details><summary>Hint</summary>

Weigh characters by position.

</details>

<details><summary>Solution</summary>

```csharp
private static char Checksum(string body)
{
    int sum = 0;
    for (int i = 0; i < body.Length; i++)
    {
        int value = Alphabet.IndexOf(body[i]);
        if (value < 0) return '\0';
        // Position weights make most swaps of neighbours visible.
        sum = (sum + value * (i + 1)) % Alphabet.Length;
    }

    return Alphabet[sum];
```

It catches most single typos and swaps, not all: the modulus 62 is not prime. File: [Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Idempotent creation

**Task:** the same owner posting the same address gets the same code.

<details><summary>Hint</summary>

Key the result by owner and address.

</details>

<details><summary>Solution</summary>

```csharp
public string Create(string owner, string url)
{
    // The separator cannot appear in an owner id, so different pairs do not merge.
    string key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{owner}\n{url}")));
    lock (_gate)
    {
        if (!_codes.TryGetValue(key, out string? code))
            _codes[key] = code = nextCode();
        return code;
```

In a service the key must be stored with a unique index, not in memory.

</details>

## 3. 🔴 Choosing the length

**Task:** compute the chance of a collision for a billion links with 7 characters.

<details><summary>Hint</summary>

Stored divided by the size of the space.

</details>

<details><summary>Solution</summary>

```csharp
public static class Keyspace
{
    public static double Size(int length) => Math.Pow(62, length);

    public static double CollisionChance(long stored, int length) => stored / Size(length);
}
```

The result is about 0.00028, that is 0.028% per new code.

</details>
