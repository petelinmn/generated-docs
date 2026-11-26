---
title: "Security architecture: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 15-security-architecture
part: 3
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Security architecture: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** HTTP, web APIs · **Time:** ~2 h

## Why it matters

Fixing a vulnerable architecture after an incident is far more expensive than asking the questions in advance.

## Threat modeling: STRIDE

| Threat | Essence | Typical defence |
|---|---|---|
| Spoofing | posing as someone else | authentication |
| Tampering | changing data | signatures, permissions, TLS |
| Repudiation | denying an action | auditing |
| Information disclosure | revealing data | encryption, authorization |
| Denial of service | making unavailable | limits |
| Elevation of privilege | gaining extra rights | least privilege |

## Object level authorization

Checking that the user signed in is not enough: they can substitute someone else's identifier.

```csharp
// "Not found" for a forbidden object does not reveal that it exists.
public static Document? Find(User user, int id, IReadOnlyDictionary<int, Document> documents) =>
    documents.TryGetValue(id, out Document? d) && CanRead(user, d) ? d : null;
```

An administrator of another organization gets no access. File: [Ex03_ObjectAuthorization.cs](code/Examples/Ex03_ObjectAuthorization.cs).

## Password storage

```csharp
byte[] salt = RandomNumberGenerator.GetBytes(16);
byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
```

The salt is unique per password, the parameters are stored with the hash, the comparison is `CryptographicOperations.FixedTimeEquals`. File: [Ex01_Passwords.cs](code/Examples/Ex01_Passwords.cs). This is a teaching example: in ASP.NET Core use Identity.

| Algorithm (OWASP recommendations) | Parameters |
|---|---|
| Argon2id (preferred) | at least 19 MiB, 2 iterations, 1 thread |
| scrypt | N=2^17, r=8, p=1 |
| PBKDF2-HMAC-SHA256 (when FIPS is required) | 600,000 iterations |

## Beginner mistakes

- ❌ `SHA256(password)` → ✅ a slow salted algorithm.
- ❌ Checking only that the user signed in → ✅ permissions on the object.
- ❌ A secret in the repository → ✅ a secret store.
- ❌ Hiding a button instead of checking on the server → ✅ check on the server.

## Related materials

- [Advanced theory](advanced.en.md)
- [JWT authentication](../../../csharp/recipes/jwt-auth/overview.en.md)

## Sources

- [OWASP: Password Storage](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [STRIDE](https://learn.microsoft.com/azure/security/develop/threat-modeling-tool-threats)
