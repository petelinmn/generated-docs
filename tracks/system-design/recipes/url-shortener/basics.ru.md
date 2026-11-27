---
title: "Сокращатель ссылок: базовая теория"
lang: ru
track: system-design
kind: recipe
doc: basics
slug: url-shortener
part: 2
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# Сокращатель ссылок: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** minimal API, [17 · Разбор проектных задач](../../modules/17-design-case-studies/basics.ru.md) · **Время:** ~1 ч

## Что строим

| Запрос | Ответ |
|---|---|
| `POST /links` с `{ "url": "…" }` | `201 Created` и код |
| `GET /{code}` | `302 Found` с `Location` или `404` |

## Код ссылки

```csharp
public static string Random(int length = 7) => RandomNumberGenerator.GetString(Alphabet, length);
```

Семь символов base62 дают около 3,5 триллиона вариантов. Случайные коды не выдают число ссылок, и их нельзя обойти по порядку. Секретной ссылке всё равно нужна проверка доступа: короткий код — не пароль.

## Сервис

```csharp
app.MapPost("/links", (CreateLink request) =>
{
    string code = ShortCode.Random();
    while (!links.TryAdd(code, request.Url))
        code = ShortCode.Random();
    return Results.Created($"/{code}", new { code });
});

app.MapGet("/{code}", (string code) =>
    links.TryGetValue(code, out string? url) ? Results.Redirect(url) : Results.NotFound());
```

`TryAdd` сохраняет пару, только если код свободен; иначе берётся новый. `Results.Redirect` по умолчанию отвечает 302.

Файл: [Ex01_Minimal.cs](code/Examples/Ex01_Minimal.cs).

## Чего не хватает

Ссылки пропадают при перезапуске, принимается любая строка, создание ничем не ограничено. Это исправляет [продвинутый уровень](advanced.ru.md).

## Запуск

```bash
cd code
dotnet test --filter "FullyQualifiedName~Minimal"
```

## Ошибки новичка

- ❌ Последовательные коды для закрытых ссылок → ✅ случайные коды и проверка доступа.
- ❌ «Проверить, потом добавить» → ✅ одна атомарная операция.
- ❌ Везде 301 → ✅ 302, пока важны переходы и адрес может меняться.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [HybridCache](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Rate limiting middleware](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [RFC 9110, redirection](https://www.rfc-editor.org/rfc/rfc9110#name-redirection-3xx)
- [Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [Microsoft.Data.Sqlite errors](https://learn.microsoft.com/dotnet/standard/data/sqlite/database-errors)
