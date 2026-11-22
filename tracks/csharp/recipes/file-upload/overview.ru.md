---
title: "Загрузка файлов"
lang: ru
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

# Загрузка файлов

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рецепт: принять файл от клиента, сохранить и отдать обратно в ASP.NET Core Minimal API. `IFormFile` для небольших форм, потоковая запись `Request.Body` и `MultipartReader` для больших; скачивание с поддержкой `Range`. Клиент контролирует имя, размер и содержимое, поэтому главное в рецепте — безопасность: серверные имена, лимиты, проверка сигнатуры, antiforgery. Облачные хранилища в этом материале не запускались.

## Ключевые идеи

- `IFormFile` для `multipart/form-data`; `Path.GetFileName` + серверное имя.
- Потоковое копирование вместо буферизации в памяти.
- Лимиты: `Kestrel.Limits.MaxRequestBodySize` → `413`.
- Расширение и `Content-Type` недостоверны — проверяйте сигнатуру.
- Antiforgery для браузерных форм (без токена — `400`).
- `enableRangeProcessing` → `206` и докачка.
- В кластере нужно общее хранилище, а не локальный диск.

## Минимальный пример

```csharp
app.MapPost("/upload", async (IFormFile file) =>
{
    var id = Guid.NewGuid().ToString("N") + Path.GetExtension(Path.GetFileName(file.FileName));
    await using var fs = File.Create(Path.Combine(root, id));
    await file.CopyToAsync(fs);
    return Results.Ok(new { id, size = file.Length });
}).DisableAntiforgery();
```

## Когда применять, а когда нет

- ✅ Аватары, вложения, импорт файлов небольшого и среднего размера.
- ✅ Отдача файлов с проверкой прав.
- ✅ Большие файлы потоком или напрямую в объектное хранилище.
- ❌ Доверять имени и типу от клиента.
- ❌ Хранить загрузки в `wwwroot` или на локальном диске при нескольких экземплярах.
- ❌ Гигабайтные файлы одним запросом без возобновления.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Минимум](basics.ru.md) | `IFormFile`, потоковая загрузка, лимит, скачивание с `Range` |
| [2. Production](advanced.ru.md) | проверка типа, antiforgery, `MultipartReader`, безопасность путей |
| [3. Масштаб и грани](expert.ru.md) | объектные хранилища, подписанные ссылки, подвохи, 10 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp file-upload level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp file-upload level 6` |

## Связанные материалы

- Пререквизиты: [10 · async/await](../../modules/10-async-await/overview.ru.md), [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.ru.md)
- Рядом: [JWT-аутентификация](../jwt-auth/overview.ru.md), [Rate limiting](../rate-limiting/overview.ru.md), [Фоновые задачи](../background-jobs/overview.ru.md)
- В других треках: [Linux](../../../linux/overview.ru.md) — права файловой системы
- Рецепты и библиотеки: [Real-time через SignalR](../signalr-realtime/overview.ru.md)
