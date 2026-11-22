---
title: "Quiz: File upload"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: file-upload
part: 2
tags: [file-upload, iformfile, multipart, path-traversal, range-requests]
status: draft
updated: 2026-11-22
---

# Quiz: File upload

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 Which parameter type accepts a file from `multipart/form-data` in Minimal API?

- A) `Stream`
- B) `IFormFile`
- C) `byte[]`
- D) `FileInfo`

<details><summary>Answer</summary>

**B.** `IFormFile` is bound from the form; `file.Length`, `CopyToAsync` and other members give access to the content.
→ [Minimal](basics.en.md#upload-with-iformfile)

</details>

## 2. 🟢 What does `Path.GetFileName("../../evil.txt")` return?

- A) `../../evil.txt`
- B) `evil.txt`
- C) an empty string
- D) an exception

<details><summary>Answer</summary>

**B.** The directory part is dropped — verified in the example. But it is better not to use the client's name on disk at all.
→ [Minimal](basics.en.md#upload-with-iformfile)

</details>

## 3. 🟢 Which code is returned when the body exceeds the Kestrel limit?

- A) `400`
- B) `413`
- C) `500`
- D) `200`

<details><summary>Answer</summary>

**B.** Verified: with `MaxRequestBodySize = 1 MB` an upload of 2 MB → `413 Payload Too Large`.
→ [Minimal](basics.en.md#the-size-limit)

</details>

## 4. 🟢 What does `enableRangeProcessing: true` in `Results.File` give?

- A) compression
- B) support for `Range` requests (a `206` response)
- C) browser caching
- D) encryption

<details><summary>Answer</summary>

**B.** Verified: `Accept-Ranges: bytes`, `Range: bytes=0-99` → `206`, length 100.
→ [Minimal](basics.en.md#download)

</details>

## 5. 🟡 How do you reliably determine that `x.png` really is a PNG?

- A) by the client's `Content-Type`
- B) by the extension
- C) by the content's signature (magic bytes)
- D) by the size

<details><summary>Answer</summary>

**C.** Verified: `x.png` with arbitrary bytes passed the extension check but the signature check rejected it.
→ [Production](advanced.en.md#checking-the-file-type)

</details>

## 6. 🟡 What happens with `UseAntiforgery()` and a form without a token on an `IFormFile` endpoint?

- A) `200`
- B) `400`
- C) `500`
- D) `404`

<details><summary>Answer</summary>

**B.** Verified; an endpoint with `DisableAntiforgery()` answered `200`.
→ [Production](advanced.en.md#antiforgery)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) a server file name (GUID) protects from overwriting others' files
- [ ] B) `IFormFile` does not buffer the form
- [ ] C) `MultipartReader` lets you read sections as a stream
- [ ] D) it is convenient to store files in `wwwroot`

<details><summary>Answer</summary>

**A, C.** B is wrong: `IFormFile` buffers; D is unsafe — store outside the web root and serve through a checking endpoint.
→ [Production](advanced.en.md#path-safety-and-storage)

</details>

## 8. 🟡 Open question: describe a safe avatar upload.

<details><summary>Key points of a good answer</summary>

- an allow-list of extensions (png/jpg) and a signature check, a size limit;
- a server name, storage outside the web root or in object storage;
- authentication + antiforgery (or a header token);
- rate limiting uploads, re-processing/re-encoding the image;
- serving with a safe `Content-Type`.

</details>

## 9. 🔴 Upload works locally but behind a balancer sometimes "loses" files. The cause?

- A) an `IFormFile` bug
- B) the file lies on one instance's disk and the next request lands on another
- C) the limit is too large
- D) no antiforgery

<details><summary>Answer</summary>

**B.** You need shared storage: object (S3/Blob) or a network volume.
→ [Scale and edges](expert.en.md#scale)

</details>

## 10. 🔴 Why serve uploaded files with `Content-Disposition: attachment`?

- A) to save traffic
- B) so that uploaded HTML/SVG is not executed on your domain (XSS)
- C) for compression
- D) for caching

<details><summary>Answer</summary>

**B.** Otherwise the browser displays active content on behalf of your domain.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>
