---
title: "File upload: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: file-upload
part: 2
tags: [file-upload, resumable-upload, presigned-url, object-storage, range-requests, interview-questions]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# File upload: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~30 min.

## Scale

- **A local disk does not scale.** Behind a balancer there are several instances: a file uploaded to one is invisible to the others. You need shared storage: object storage (S3/Blob), a network volume.
- **Signed links.** The client uploads/downloads straight to object storage via a short-lived URL; your API does not pass the bytes through itself and does not become the bottleneck.
- **Resumable upload.** For gigabyte files — uploading in chunks with a retry of the missing ones; protocols like tus, S3 multipart upload.
- **Asynchronous post-processing.** After the upload (scanning, previews, transcoding) a job goes to a queue ([Background jobs](../background-jobs/overview.en.md)).
- **Deduplication and hashes.** A content hash (SHA-256) as the key — identical files are stored once.
- **Cleanup.** Unfinished uploads and "orphaned" files are removed by a background job.

> ⚠️ Verify: tus, S3 multipart upload and signed links were not run in this material; check the documentation of your chosen storage.

## Edges and gotchas

**1. Limits at several levels.** Verified: Kestrel (`MaxRequestBodySize`) returned `413` at 2 MB with a 1 MB limit. But the form has its own limits (`FormOptions`), and in front of the application there are proxy/balancer limits (for example nginx has its own `client_max_body_size`). The strictest one wins.

**2. A file name is input.** `../../evil.txt` became `evil.txt` only thanks to `Path.GetFileName` (verified). `Path.Combine` with an absolute second argument discards the first — another route to traversal if you take the whole path from the client.

**3. `Content-Type` and the extension are not proof.** Verified: `x.png` with arbitrary bytes passed the extension check; only the signature check rejected it.

**4. `IFormFile` buffers.** Large forms go to disk/memory; for gigabytes — `MultipartReader` and a streaming write ([production](advanced.en.md#streaming-multipart-upload)).

**5. Antiforgery.** Without a token the form returns `400` (verified); a careless `DisableAntiforgery()` with cookie authentication opens CSRF.

**6. Range requests.** `enableRangeProcessing: true` gives `206` and `Content-Range` (verified); without it the client cannot resume a download. Range does not work for streams without a length.

**7. Client abort.** On a disconnect `HttpContext.RequestAborted` is cancelled; a half-written file must be deleted or debris remains.

**8. Name races.** A server GUID rules out collisions; a client name means overwriting others' files.

**9. Executable content.** Uploaded HTML/SVG served as `text/html` from your domain is XSS. Serve it as `attachment` or from a cookie-less domain.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `IFormFile` | simple | buffering, form limits |
| Streaming `Request.Body` | minimal memory | no multipart metadata |
| `MultipartReader` | stream + metadata | more code |
| Direct upload to object storage | scale, no load on the API | signed links, callbacks |
| Resumable upload (tus) | reliable for big files | more complex client and server |

## Interview questions

<details><summary>1. How do you accept a file in Minimal API?</summary>

An `IFormFile` (or `IFormFileCollection`) parameter from `multipart/form-data`; for API endpoints — `DisableAntiforgery()` or a token.

</details>

<details><summary>2. Why can't you use the client's file name as a path?</summary>

Path traversal: `../../…` allows writing outside the directory. Generate the name on the server; `Path.GetFileName` drops the directory part (verified).

</details>

<details><summary>3. How do you limit the upload size?</summary>

`Kestrel.Limits.MaxRequestBodySize`, form limits (`FormOptions`) and endpoint attributes/metadata; in practice also proxy limits. Verified: `413` on excess.

</details>

<details><summary>4. How is a streaming upload better than `IFormFile`?</summary>

It does not buffer everything in memory/a temp file; memory does not depend on the file size.

</details>

<details><summary>5. How do you check the real type of a file?</summary>

An extension allow-list + a signature (magic bytes) + parsing/scanning if needed; the client's `Content-Type` is untrustworthy (verified on a fake PNG).

</details>

<details><summary>6. What is `enableRangeProcessing` for?</summary>

For `Range` requests: resuming downloads, video seeking; a `206 Partial Content` response with `Content-Range` (verified).

</details>

<details><summary>7. Why antiforgery for file uploads?</summary>

With cookie authentication a form is vulnerable to CSRF; without a token the request gets `400` (verified). For APIs with a header token CSRF is irrelevant.

</details>

<details><summary>8. How do you scale file storage?</summary>

Object storage instead of the local disk, signed links for direct upload/download, metadata in the DB, background post-processing.

</details>

<details><summary>9. Trick question: is a file `x.png` with an extension from the allow-list safe?</summary>

No: the extension does not prove the content (verified: arbitrary bytes passed the extension check). You need a signature check and safe serving.

</details>

<details><summary>10. Trick question: upload works locally but "loses" files behind a balancer. Why?</summary>

The file lies on one instance's disk and the next request lands on another. You need shared storage.

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — storage and boundaries.
- [Background jobs](../background-jobs/overview.en.md) — file post-processing.
- [Kubernetes](../../../kubernetes/overview.en.md) — volumes and ingress limits.

## Sources

- [File uploads in ASP.NET Core](https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads)
- [Kestrel limits](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/options)
- [OWASP: File Upload Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html)
