---
title: "Квиз: проектирование API"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 05-api-design
part: 1
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: проектирование API

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой метод не объявлен идемпотентным?

- A) GET
- B) PUT
- C) DELETE
- D) POST

<details><summary>Ответ</summary>

**D.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что вернуть после создания ресурса?

- A) 200 без заголовков
- B) 201 с `Location`
- C) 302
- D) 409

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Зачем `Idempotency-Key`?

- A) Для аутентификации
- B) Чтобы распознать повтор того же запроса
- C) Для пагинации
- D) Для сжатия

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Какой тип содержимого у problem details в JSON?

- A) `application/error+json`
- B) `text/problem`
- C) `application/problem+json`
- D) `application/json-patch+json`

<details><summary>Ответ</summary>

**C.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Клиент обновил ресурс с устаревшим `If-Match`. Код?

- A) 200
- B) 412
- C) 428
- D) 404

<details><summary>Ответ</summary>

**B.** 428 — когда условия нет вовсе. → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Выбери все совместимые изменения protobuf

- [ ] A) Добавить поле в ответ
- [ ] B) Изменить номер поля
- [ ] C) Добавить метод
- [ ] D) Переименовать сервис

<details><summary>Ответ</summary>

**A, C.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что выведет код?

```csharp
List<Post> posts = [new(1, "a"), new(2, "b"), new(3, "c"), new(4, "d")];
var first = Pagination.ByOffset(posts, 0, 2);
posts.Add(new Post(5, "e"));
var second = Pagination.ByOffset(posts, 2, 2);
Console.WriteLine(string.Join(" ", second.Select(p => p.Id)));
```

<details><summary>Ответ</summary>

```text
3 2
```

Пост 3 уже был на первой странице. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Найди ошибку

«Если запрос не удался, клиент генерирует новый idempotency key и повторяет».

<details><summary>Ответ</summary>

Сервер не узнает повтор и выполнит операцию второй раз. Ключ создаётся один раз на намерение. → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что выведет код?

```csharp
Console.WriteLine(JsonSerializer.Serialize(new { Day = DayOfWeek.Monday }));
```

<details><summary>Ответ</summary>

```text
{"Day":1}
```

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Спроектируйте API платежа, переживающий потерю ответа.

<details><summary>Ключевые пункты ответа</summary>

- Ключ идемпотентности, отпечаток тела, срок хранения.
- Атомарный захват и запись вместе с эффектом.
- Что делать, если внешний провайдер не ответил: состояние «неизвестно» и сверка.
- Коды 400, 409, 422 и 202.

</details>
