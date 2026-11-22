---
title: "File upload"
lang: en
track: csharp
kind: recipe
doc: overview
slug: file-upload
part: 2
levels: [1, 2, 3, 4]
tags: [file-upload, iformfile, multipart, streaming, range-requests, path-traversal]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# File upload

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: accept a file from a client, save it and serve it back in an ASP.NET Core Minimal API. `IFormFile` for small forms, a streaming write of `Request.Body` and `MultipartReader` for large ones; downloads with `Range` support. The client controls the name, size and content, so the main point of the recipe is security: server names, limits, a signature check, antiforgery. Cloud storage was not run in this material.

## Key ideas

- `IFormFile` for `multipart/form-data`; `Path.GetFileName` + a server name.
- Streaming copy instead of buffering in memory.
- Limits: `Kestrel.Limits.MaxRequestBodySize` → `413`.
- The extension and `Content-Type` are untrustworthy — check the signature.
- Antiforgery for browser forms (without a token — `400`).
- `enableRangeProcessing` → `206` and resumable downloads.
- A cluster needs shared storage, not a local disk.

## Minimal example

```csharp
app.MapPost("/upload", async (IFormFile file) =>
{
    var id = Guid.NewGuid().ToString("N") + Path.GetExtension(Path.GetFileName(file.FileName));
    await using var fs = File.Create(Path.Combine(root, id));
    await file.CopyToAsync(fs);
    return Results.Ok(new { id, size = file.Length });
}).DisableAntiforgery();
```

## When to use and when not

- ✅ Avatars, attachments, importing small and medium files.
- ✅ Serving files with permission checks.
- ✅ Large files as a stream or directly to object storage.
- ❌ Trusting the client's name and type.
- ❌ Keeping uploads in `wwwroot` or on a local disk with several instances.
- ❌ Gigabyte files in one request without resume.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | `IFormFile`, streaming upload, a limit, download with `Range` |
| [2. Production](advanced.en.md) | type checks, antiforgery, `MultipartReader`, path safety |
| [3. Scale and edges](expert.en.md) | object storage, signed links, gotchas, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp file-upload level 5` |
| 6. Tests | not generated yet — `/learn csharp file-upload level 6` |

## Related materials

- Prerequisites: [10 · async/await](../../modules/10-async-await/overview.en.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md)
- Nearby: [JWT authentication](../jwt-auth/overview.en.md), [Rate limiting](../rate-limiting/overview.en.md), [Background jobs](../background-jobs/overview.en.md)
- In other tracks: [Linux](../../../linux/overview.en.md) — file system permissions
- Recipes and libraries: [Real-time with SignalR](../signalr-realtime/overview.en.md)
