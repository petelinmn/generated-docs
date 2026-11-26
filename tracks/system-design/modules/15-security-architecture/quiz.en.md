---
title: "Quiz: security architecture"
lang: en
track: system-design
kind: module
doc: quiz
slug: 15-security-architecture
part: 3
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: security architecture

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does T stand for in STRIDE?

- A) Timeout
- B) Tampering
- C) Tracing
- D) Token

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 A signed-in user requested someone else's invoice. What is needed?

- A) Nothing, they signed in
- B) A permission check on this object
- C) A CAPTCHA
- D) Compression

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 3. 🟢 How should passwords be stored?

- A) As plain text
- B) SHA-256 without a salt
- C) A slow hash with a unique salt
- D) In Base64

<details><summary>Answer</summary>

**C.** Base64 is not protection. → [Basics](basics.en.md)

</details>

## 4. 🟢 What does OWASP recommend first?

- A) Argon2id
- B) MD5
- C) SHA-1
- D) CRC32

<details><summary>Answer</summary>

**A.** → [Basics](basics.en.md)

</details>

## 5. 🟡 What protects against replaying a request?

- [ ] A) A nonce
- [ ] B) A timestamp with a maximum age
- [ ] C) Compression
- [ ] D) Only an HMAC of the body

<details><summary>Answer</summary>

**A, B.** A signature alone does not stop a replay. → [Advanced](advanced.en.md)

</details>

## 6. 🟡 The main idea of zero trust?

- A) The internal network is trusted
- B) Every request is checked regardless of the network
- C) Give nobody access
- D) VPN only

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 A server downloads an image from a user's URL. The main risk?

- A) XSS
- B) SSRF
- C) CSRF
- D) Stack overflow

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does this code print?

```csharp
var user = new User("eve", "other", IsAdmin: true);
Console.WriteLine(DocumentAccess.CanRead(user, new Document(1, "acme", "ann")));
```

<details><summary>Answer</summary>

```text
False
```

An administrator of another organization. → [Basics](basics.en.md)

</details>

## 9. 🔴 What does this code print on Windows?

```csharp
Console.WriteLine(Path.Combine("C:\\data", "C:\\Windows\\win.ini"));
```

<details><summary>Answer</summary>

```text
C:\Windows\win.ini
```

→ [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

Do threat modeling for user file uploads.

<details><summary>Key points of a good answer</summary>

- The data flow and trust boundaries.
- STRIDE threats: name spoofing, size, content, access to others' files.
- Limits, generated names, permission checks, storage outside the web root.
- Auditing and residual risks.

</details>
