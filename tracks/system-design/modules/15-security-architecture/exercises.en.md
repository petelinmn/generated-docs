---
title: "Exercises: security architecture"
lang: en
track: system-design
kind: module
doc: exercises
slug: 15-security-architecture
part: 3
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Exercises: security architecture

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Hiding secrets in headers

**Task:** replace the values of sensitive headers with `***` regardless of case.

**Expected result:**

```text
authorization -> ***; Accept stays
```

<details><summary>Hint</summary>

`StringComparer.OrdinalIgnoreCase`.

</details>

<details><summary>Solution</summary>

```csharp
headers.ToDictionary(
    h => h.Key,
    h => Sensitive.Contains(h.Key, StringComparer.OrdinalIgnoreCase) ? "***" : h.Value);
```

A deny list easily falls behind; logging only allowed headers is safer. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 An allow list for outgoing addresses

**Task:** allow only `https` to exactly listed hosts, without userinfo or a non-default port.

**Expected result:**

```text
https://api.example.com/v1 -> true; look-alike hosts, http, other ports -> false
```

<details><summary>Hint</summary>

Parse a `Uri`, do not compare strings.

</details>

<details><summary>Solution</summary>

```csharp
Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
&& uri.Scheme == Uri.UriSchemeHttps
&& uri.IsDefaultPort
&& uri.UserInfo.Length == 0
&& allowedHosts.Contains(uri.Host);
```

This is one layer: redirects and the addresses a name resolves to also have to be controlled.

</details>

## 3. 🔴 A path inside the root

**Task:** return the full path only if it stays inside the root folder.

**Expected result:**

```text
sub/report.txt -> allowed; ../secret.txt, ../data-evil/x -> null
```

<details><summary>Hint</summary>

Normalize the path and compare with a root ending in a separator.

</details>

<details><summary>Solution</summary>

```csharp
string full = Path.GetFullPath(Path.Combine(fullRoot, name));
// Ordinal is the strict choice; symbolic links are not resolved here.
return full.StartsWith(fullRoot, StringComparison.Ordinal) ? full : null;
```

It is better not to use a user's name as a path at all and to issue your own.

</details>
