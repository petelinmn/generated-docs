---
title: "Упражнения: Загрузка файлов"
lang: ru
track: csharp
kind: recipe
doc: exercises
slug: file-upload
part: 2
tags: [file-upload, iformfile, streaming, multipart-reader, range-requests]
status: draft
updated: 2026-11-22
---

# Упражнения: Загрузка файлов

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами, потом открывайте подсказки. Решения проверены запуском (Kestrel + `HttpClient`, без внешних пакетов). Запускаемых версий в `code/` нет (уровня 5 нет).

## 1. 🟢 Безопасная загрузка

**Задача:** эндпоинт `POST /upload` сохраняет файл под серверным именем и возвращает исходное имя, размер и тип. Отправьте файл с именем `../../evil.txt`.

**Ожидаемый результат:** `200`, `original = "evil.txt"`, `size = 11`; за пределами каталога ничего не создано.

<details><summary>Подсказка</summary>

`IFormFile`, `Path.GetFileName`, `Guid.NewGuid().ToString("N")`, `.DisableAntiforgery()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: server-generated file name.
app.MapPost("/upload", async (IFormFile file) =>
{
    var safe = Path.GetFileName(file.FileName);
    var id = Guid.NewGuid().ToString("N") + Path.GetExtension(safe);
    await using var fs = File.Create(Path.Combine(root, id));
    await file.CopyToAsync(fs);
    return Results.Ok(new { id, original = safe, size = file.Length, file.ContentType });
}).DisableAntiforgery();
```

Проверено: `upload: 200 {"id":"…txt","original":"evil.txt","size":11,"contentType":"text/plain"}`, вне корня файл не создан.

</details>

## 2. 🟡 Потоковая загрузка с лимитом

**Задача:** `PUT /stream/{name}` пишет тело на диск; лимит запроса — 1 МБ. Отправьте 500 КБ и 2 МБ.

**Ожидаемый результат:** `200 {"saved":500000}` и `413`.

<details><summary>Подсказка</summary>

`req.Body.CopyToAsync(fs)` и `ConfigureKestrel(k => k.Limits.MaxRequestBodySize = …)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: streaming upload with a body size limit.
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 1024 * 1024);
app.MapPut("/stream/{name}", async (string name, HttpRequest req) =>
{
    await using var fs = File.Create(Path.Combine(root, Path.GetFileName(name)));
    await req.Body.CopyToAsync(fs);
    return Results.Ok(new { saved = fs.Length });
});
```

Проверено: `stream 500KB: 200 {"saved":500000}`, `stream 2MB (limit 1MB): 413`.

</details>

## 3. 🟡 Проверка типа

**Задача:** принимайте только `.png/.jpg/.txt`; для `.png` проверяйте сигнатуру `89 50 4E 47`. Проверьте три файла: `x.exe`, `x.png` с произвольными байтами, `x.png` с настоящей сигнатурой.

**Ожидаемый результат:** `extension not allowed`, `content is not PNG`, `ok`.

<details><summary>Подсказка</summary>

`file.OpenReadStream()`, `ReadExactlyAsync` на 4 байта, `SequenceEqual`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: extension allow-list + magic-bytes check.
var allowed = new[] { ".png", ".jpg", ".txt" };
var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
if (!allowed.Contains(ext)) return Results.BadRequest("extension not allowed");
var head = new byte[4];
await using var s = file.OpenReadStream();
await s.ReadExactlyAsync(head);
var isPng = head.AsSpan().SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
return ext == ".png" && !isPng ? Results.BadRequest("content is not PNG") : Results.Ok("ok");
```

Проверено: три ответа — `"extension not allowed"`, `"content is not PNG"`, `"ok"`.

</details>

## 4. 🔴 Скачивание с докачкой

**Задача:** `GET /download/{name}` отдаёт файл с поддержкой `Range`. Запросите байты 0–99 файла в 500 000 байт.

**Ожидаемый результат:** `206`, длина 100, `Content-Range: bytes 0-99/500000`; для несуществующего файла — `404`.

<details><summary>Подсказка</summary>

`Results.File(path, contentType, fileName, enableRangeProcessing: true)`; заголовок `RangeHeaderValue(0, 99)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: download with HTTP range support.
app.MapGet("/download/{name}", (string name) =>
{
    var path = Path.Combine(root, Path.GetFileName(name));
    return File.Exists(path)
        ? Results.File(path, "application/octet-stream", name, enableRangeProcessing: true)
        : Results.NotFound();
});
```

Проверено: `range 0-99: 206 length=100 content-range=bytes 0-99/500000`, `download missing: 404`.

</details>
