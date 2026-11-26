---
title: "Упражнения: проектирование API"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 05-api-design
part: 1
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: проектирование API

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Можно ли повторить запрос

**Задача:** по методу и наличию ключа идемпотентности решить, можно ли повторять автоматически.

**Ожидаемый результат:**

```text
get -> true, POST -> false, PATCH -> false, POST with key -> true
```

<details><summary>Подсказка</summary>

Вспомните список идемпотентных методов.

</details>

<details><summary>Решение</summary>

```csharp
public static bool IsRetrySafe(string method, bool hasIdempotencyKey) =>
    method.ToUpperInvariant() is "GET" or "HEAD" or "OPTIONS" or "TRACE" or "PUT" or "DELETE"
    || hasIdempotencyKey;
```

Ключ помогает, только если сервер его поддерживает. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Безопасный разбор курсора

**Задача:** разобрать курсор без исключений при любом вводе.

**Ожидаемый результат:**

```text
"NDI" -> true, 42; "###", "YWJj", null -> false
```

<details><summary>Подсказка</summary>

Две проверки: корректный Base64Url и число внутри.

</details>

<details><summary>Решение</summary>

```csharp
id = 0;
if (string.IsNullOrEmpty(cursor) || !Base64Url.IsValid(cursor)) return false;

string text = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(cursor));
return long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out id);
```

Кодирование не защищает: клиент может подставить любой номер, права проверяются отдельно.

</details>

## 3. 🔴 Проверка If-Match

**Задача:** поддержать `*`, список тегов и отсутствие ресурса; слабые теги не совпадают.

**Ожидаемый результат:**

```text
"*" with "1" -> true; list "1","2" with "2" -> true; weak -> false; no resource -> false
```

<details><summary>Подсказка</summary>

Сначала отсутствие ресурса, потом `*`.

</details>

<details><summary>Решение</summary>

```csharp
if (currentETag is null) return false;          // nothing to match
if (header.Trim() == "*") return true;
if (currentETag.StartsWith("W/", StringComparison.Ordinal)) return false;

return header.Split(',', StringSplitOptions.TrimEntries).Contains(currentETag);
```

Упрощение: запятая внутри тега не поддерживается; в production берите готовый разбор заголовков.

</details>
