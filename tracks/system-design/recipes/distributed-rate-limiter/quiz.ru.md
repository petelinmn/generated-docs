---
title: "Квиз: распределённый rate limiter"
lang: ru
track: system-design
kind: recipe
doc: quiz
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Квиз: распределённый rate limiter

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Три экземпляра разрешают по 100 в минуту. Клиент может получить

- A) 100
- B) 300
- C) 33
- D) без ограничений

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что ограничивает ёмкость token bucket?

- A) Среднюю скорость
- B) Размер всплеска
- C) Число клиентов
- D) Таймаут

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Какой код означает «слишком много запросов»?

- A) 403
- B) 429
- C) 503
- D) 409

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟡 Зачем скрипту `TIME`?

- A) Это быстрее
- B) Одни часы для всех экземпляров
- C) В Lua нет чисел
- D) Чтобы удалять ключи

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 5. 🟡 Что получит клиент от `return 3.9` в скрипте?

- A) 3.9
- B) 3
- C) 4
- D) ошибку

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Выбери все верные утверждения о скриптах

- [ ] A) Выполняются атомарно
- [ ] B) Сервер заблокирован на время выполнения
- [ ] C) Ключи передают в `KEYS`
- [ ] D) Выполняются параллельно с другими командами

<details><summary>Ответ</summary>

**A, B, C.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Найди проблему

```csharp
var left = (int)await redis.StringGetAsync(key);
if (left > 0) { await redis.StringSetAsync(key, left - 1); return true; }
return false;
```

<details><summary>Ответ</summary>

Два экземпляра прочитают одно значение; сделайте это одним атомарным шагом.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🔴 Проверка не уложилась в таймаут. Токен

- A) точно не списан
- B) возможно, списан
- C) точно списан
- D) возвращён автоматически

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 9. 🔴 Чем рискует аренда пачки?

- A) Порчей данных
- B) Отказами при неиспользованных токенах в другом месте
- C) Взаимной блокировкой в Redis
- D) Большей задержкой

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Для попыток входа при недоступном Redis обычно безопаснее

- A) пропускать всех
- B) запрещать или строгий локальный лимит
- C) без лимита
- D) повторять бесконечно

<details><summary>Ответ</summary>

**B.**

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Открытый вопрос: спроектируй лимиты публичного API с бесплатным и платным тарифами.

<details><summary>Ключевые пункты ответа</summary>

- Ключ по учётной записи, а не по адресу; тариф задаёт ёмкость и скорость.
- Несколько лимитов (минута, сутки) проверяются вместе.
- 429 с `Retry-After`; показывать остаток.
- Политика отказа для каждого endpoint; мониторинг отказов.

→ [Экспертная теория](expert.ru.md)

</details>
