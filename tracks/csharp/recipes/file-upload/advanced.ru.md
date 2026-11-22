---
title: "Загрузка файлов: production"
lang: ru
track: csharp
kind: recipe
doc: advanced
slug: file-upload
part: 2
tags: [file-upload, multipart-reader, antiforgery, content-sniffing, path-traversal, security]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# Загрузка файлов: production

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Минимум](basics.ru.md) · **2. Production** · [3. Масштаб и грани](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [минимальное решение](basics.ru.md). **Время:** ~45 мин.

## Проверка типа файла

Клиент сам указывает имя и `Content-Type` — им нельзя доверять. Минимум:

1. белый список расширений;
2. проверка «магических байтов» содержимого;
3. лимит размера.

```csharp
app.MapPost("/sniff", async (IFormFile file) =>
{
    var allowed = new[] { ".png", ".jpg", ".txt" };
    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowed.Contains(ext)) return Results.BadRequest("extension not allowed");
    var head = new byte[4];
    await using var s = file.OpenReadStream();
    await s.ReadExactlyAsync(head);
    var isPng = head.AsSpan().SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
    return ext == ".png" && !isPng ? Results.BadRequest("content is not PNG") : Results.Ok("ok");
}).DisableAntiforgery();
```

Проверено: `x.exe` → `extension not allowed`; `x.png` с произвольными байтами → `content is not PNG`; настоящая PNG-сигнатура → `ok`. Это защита первого уровня: для критичных случаев (изображения, документы) используйте библиотеки разбора и антивирусное сканирование.

> ⚠️ Проверить: полный набор сигнатур форматов и рекомендации по сканированию вредоносного содержимого в этом материале не запускались.

## Antiforgery

`IFormFile`-эндпоинты в Minimal API по умолчанию требуют antiforgery-токен. Проверено: с `AddAntiforgery()` + `UseAntiforgery()` форма без токена → `400`, а эндпоинт с `DisableAntiforgery()` → `200`. Для браузерных форм оставляйте проверку включённой и отдавайте токен через `IAntiforgery`; отключайте только для API с аутентификацией по заголовку (токен, а не cookie) — там CSRF не применим.

## Потоковая многочастная загрузка

`IFormFile` буферизует форму (память или временный файл). Для очень больших файлов читайте multipart потоком:

```csharp
var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(req.ContentType).Boundary).Value!;
var reader = new MultipartReader(boundary, req.Body);
while (await reader.ReadNextSectionAsync() is { } section)
{
    var cd = ContentDispositionHeaderValue.Parse(section.ContentDisposition);
    // copy section.Body to the destination stream
}
```

Проверено: форма из двух файлов (`a.bin` — 1000 байт, `b.bin` — 50 байт) прочитана секциями: `f1:a.bin:1000;f2:b.bin:50`. Типы `MediaTypeHeaderValue`/`ContentDispositionHeaderValue` здесь — из `Microsoft.Net.Http.Headers` (не из `System.Net.Http.Headers`; при двух `using` — неоднозначность, ошибка компиляции CS0104).

## Безопасность путей и хранилища

| Правило | Зачем |
|---|---|
| Имя на диске генерирует сервер | path traversal, перезапись чужих файлов |
| `Path.GetFileName` для любого имени от клиента | отбрасывает каталоговую часть (проверено) |
| Хранить вне веб-корня | файл не исполнится и не отдастся напрямую |
| Отдавать с `Content-Disposition: attachment` и безопасным типом | защита от XSS через загруженный HTML/SVG |
| Проверять права на скачивание | не отдавать чужие файлы по угаданному id |
| Лимит размера и квоты на пользователя | защита от переполнения диска |

Для облака файлы кладут в объектное хранилище (Azure Blob, S3): сервер выдаёт клиенту короткоживущую подписанную ссылку, и файл идёт мимо вашего API.

> ⚠️ Проверить: API облачных SDK (Azure Blob/S3) и подписанные ссылки в этом материале не запускались.

## Ошибки клиента и ответы

| Ситуация | Ответ |
|---|---|
| нет части `file` | `400` (проверено) |
| тело больше лимита | `413` (проверено) |
| расширение/содержимое недопустимы | `400`/`415` |
| файл не найден | `404` (проверено) |
| нет прав | `403` |

## Best practices

| Правило | Почему |
|---|---|
| Потоковое копирование | память не растёт с размером файла |
| Белый список типов + проверка содержимого | клиентский тип недостоверен |
| Серверные имена и каталог вне веб-корня | безопасность путей |
| Лимиты тела, форм и квоты | защита ресурсов |
| Асинхронный ввод-вывод и `CancellationToken` | поток не блокируется, отмена клиента |
| Метаданные (имя, тип, размер, владелец) в БД | поиск и права |

## Связанные материалы

- Назад: [минимум](basics.ru.md). Дальше: [масштаб и грани](expert.ru.md).
- [JWT-аутентификация](../jwt-auth/overview.ru.md) — права на скачивание.
- [Rate limiting](../rate-limiting/overview.ru.md) — ограничение частоты загрузок.
- [19 · Производительность](../../modules/19-performance/overview.ru.md) — выделения памяти при копировании.

## Источники

- [File uploads: security considerations](https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads#security-considerations)
- [Antiforgery in Minimal APIs](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery)
- [`MultipartReader`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.webutilities.multipartreader)
