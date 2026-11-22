---
title: "Загрузка файлов: минимальное решение"
lang: ru
track: csharp
kind: recipe
doc: basics
slug: file-upload
part: 2
tags: [file-upload, iformfile, multipart, streaming, download, range-requests]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Загрузка файлов: минимальное решение

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Минимум** · [2. Production](advanced.ru.md) · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [10 · async/await](../../modules/10-async-await/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~30 мин.

## Задача

Принять файл от клиента, сохранить его на диск и отдать обратно. Это одновременно удобная и опасная функция: клиент контролирует имя, размер и содержимое файла. Рецепт показывает базовое решение; безопасность — в [production](advanced.ru.md).

## Загрузка через `IFormFile`

```csharp
app.MapPost("/upload", async (IFormFile file) =>
{
    var safe = Path.GetFileName(file.FileName);           // strips any directory part
    var id = Guid.NewGuid().ToString("N") + Path.GetExtension(safe);
    await using var fs = File.Create(Path.Combine(root, id));
    await file.CopyToAsync(fs);
    return Results.Ok(new { id, original = safe, size = file.Length, file.ContentType });
}).DisableAntiforgery();
```

Параметр `IFormFile` привязывается из `multipart/form-data`. Проверено: файл с содержимым `hello world` и клиентским именем `../../evil.txt` → ответ `200`, `original = "evil.txt"` (каталоговая часть отброшена `Path.GetFileName`), `size = 11`; на диске лежит файл с **серверным** именем (GUID + расширение), за пределами каталога ничего не создано. Запрос без части `file` → `400`.

Имя на диске генерируется сервером — исходное имя клиента хранится только как метаданные.

> `DisableAntiforgery()` нужен для API-эндпоинтов, которые вызываются не из HTML-форм вашего сайта. Для форм в браузере antiforgery оставляют включённым ([production](advanced.ru.md#antiforgery)).

## Потоковая загрузка

Для больших файлов не читайте форму целиком: пишите тело запроса прямо на диск:

```csharp
app.MapPut("/stream/{name}", async (string name, HttpRequest req) =>
{
    var path = Path.Combine(root, Path.GetFileName(name));
    await using var fs = File.Create(path);
    await req.Body.CopyToAsync(fs);
    return Results.Ok(new { saved = fs.Length });
});
```

Проверено: 500 000 байт → `200`, `saved = 500000`. Тело читается потоком, в памяти целиком не лежит.

## Лимит размера

Kestrel по умолчанию ограничивает тело запроса (около 30 МБ); значение задают в конфигурации:

```csharp
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 1024 * 1024);   // 1 MB
```

Проверено: с лимитом 1 МБ загрузка 2 000 000 байт → `413 Payload Too Large`.

## Скачивание

```csharp
app.MapGet("/download/{name}", (string name) =>
{
    var path = Path.Combine(root, Path.GetFileName(name));
    return File.Exists(path)
        ? Results.File(path, "application/octet-stream", name, enableRangeProcessing: true)
        : Results.NotFound();
});
```

Проверено: ответ `200`, `Content-Length = 500000`, `Accept-Ranges: bytes`; запрос `Range: bytes=0-99` → `206`, длина 100, `Content-Range: bytes 0-99/500000`; несуществующий файл → `404`. Range-запросы нужны для докачки и стриминга видео.

## Частые ошибки новичка

- ❌ Сохранять файл под именем клиента → ✅ генерировать имя на сервере (path traversal: `../../…`).
- ❌ Читать весь файл в `byte[]`/`MemoryStream` → ✅ копировать потоком.
- ❌ Доверять `ContentType` и расширению → ✅ проверять содержимое ([production](advanced.ru.md#проверка-типа-файла)).
- ❌ Не ограничивать размер → ✅ лимит запроса.
- ❌ Хранить файлы в `wwwroot` → ✅ вне публичного каталога, отдавать через эндпоинт с проверкой прав.

## Связанные материалы

- Дальше: [production](advanced.ru.md) — проверка типа, antiforgery, `MultipartReader`, хранилища.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md) — привязка параметров.
- [10 · async/await](../../modules/10-async-await/overview.ru.md) — асинхронный ввод-вывод.
- [JWT-аутентификация](../jwt-auth/overview.ru.md) — доступ к файлам по токену.
- [Linux](../../../linux/overview.ru.md) — права файловой системы.

## Источники

- [Upload files in ASP.NET Core](https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads)
- [Minimal APIs: file uploads](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding#binding-formfiles-and-form-collections)
- [Kestrel limits](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/options)
