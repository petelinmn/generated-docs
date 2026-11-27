---
title: "Распределённый rate limiter: экспертная теория"
lang: ru
track: system-design
kind: recipe
doc: expert
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Распределённый rate limiter: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Меньше обращений: аренда пачками

```csharp
int held = _leases.GetValueOrDefault(key);
if (held < cost)
{
    SharedCalls++;
    var lease = await shared.AcquireAsync(key, Math.Max(batch, cost), cancellationToken);
    if (!lease.Allowed)
    {
        // Less than a batch may remain: ask for exactly what is needed.
        SharedCalls++;
        var exact = await shared.AcquireAsync(key, cost, cancellationToken);
        return exact;
    }

    held += Math.Max(batch, cost);
}

// Tokens held here are invisible to other instances until used.
_leases[key] = held - cost;
return new Decision(true, held - cost, TimeSpan.Zero);
```

Каждый экземпляр берёт пачку и тратит локально. В тесте с ёмкостью 20 и пачками по 10 первый тратит свои десять, второй берёт остальные десять и тратит один; после этого первому отказывают, хотя девять токенов не использованы. Чем меньше пачка относительно лимита, тем меньше ошибка. В примере аренда не истекает и не возвращается. Файл: [Ex04_Leased.cs](code/Examples/Ex04_Leased.cs).

## Горячие ключи и шардирование

Все запросы по одному ключу идут на один узел Redis. Очень горячий лимит можно разделить на N ключей с частью лимита в каждом; запрос выбирает шард. Сумма сохраняется, но один шард может опустеть, пока в других есть токены.

## Кластер и сохранность

- Скрипт трогает один ключ, поэтому работает с хеш-слотами. Нескольким лимитам в одном скрипте нужен общий хеш-тег, например `{tenant1}:minute` и `{tenant1}:day`.
- Redis кэширует скрипты по SHA1; кэш не сохраняется, и `EVALSHA` может ответить `NOSCRIPT`.
- После перезапуска или failover состояние корзин может пропасть: клиенты снова получат полную корзину.

## Справочник

| Вопрос | Решение в рецепте |
|---|---|
| Алгоритм | token bucket |
| Состояние | hash с `tokens` и `at` |
| Атомарность | Lua-скрипт |
| Часы | `TIME` из Redis |
| Срок жизни | время наполнения плюс секунда |
| Отказ | 429, `Retry-After` в целых секундах |
| Сбой | настраиваемая политика и таймаут |

Использованный здесь `X-RateLimit-Remaining` — соглашение, а не стандартный заголовок.

## Подвохи

- **Дробная часть пропадает** в ответах Lua.
- **Долгий скрипт блокирует весь сервер.**
- **Таймаут не отменяет** команду.
- **Отрицательная стоимость добавляет токены**: проверяйте её.
- **Ключ из заголовка** позволяет клиенту придумывать новый ключ на каждый запрос и не попадать под лимит.
- **Словарь в процессе никогда не вытесняет** ключи.
- **Анонимные клиенты делят один ключ** в этом примере: один может израсходовать лимит всех.
- **Ключи из пользовательского ввода** могут создаваться без ограничения; срок жизни смягчает ущерб.
- **Стоимость больше ёмкости** не пройдёт никогда, хотя `Retry-After` предлагает подождать.

## Компромиссы и альтернативы

| Подход | Точность | Задержка | Зависимость |
|---|---|---|---|
| Только локально | на экземпляр | нет | нет |
| Общее на каждый запрос | высокая | обращение по сети | хранилище |
| Аренда пачками | приближённая | в среднем низкая | хранилище |
| Ограничитель шлюза | зависит от шлюза | на границе | шлюз |
| Привязка клиента к экземпляру | точная, пока держится маршрут | нет | балансировщик |

## Вопросы с собеседований

**1. Почему не хватает лимита на экземпляр?**

<details><summary>Ответ</summary>

Фактический лимит растёт с числом экземпляров и зависит от балансировки.

</details>

**2. Почему скрипт, а не `INCR`?**

<details><summary>Ответ</summary>

`INCR` хватает для фиксированного окна; token bucket читает, считает и пишет, и это должно быть атомарно.

</details>

**3. Чьи часы?**

<details><summary>Ответ</summary>

Одни, здесь `TIME` из Redis.

</details>

**4. Что делать, когда Redis недоступен?**

<details><summary>Ответ</summary>

Выбрать для операции: пропускать, запрещать или локальный лимит; короткий таймаут и оповещение.

</details>

**5. Как быть с горячим ключом?**

<details><summary>Ответ</summary>

Разделить лимит на шарды, брать пачками или отклонять локально заведомо лишнее.

</details>

**6. Чем платят за пачки?**

<details><summary>Ответ</summary>

Токены застревают в экземпляре, и бывают отказы при наличии токенов в другом.

</details>

**7. А Redis Cluster?**

<details><summary>Ответ</summary>

Ключи одного скрипта передаются явно и обычно должны быть в одном хеш-слоте.

</details>

**8. Точен ли лимит после failover?**

<details><summary>Ответ</summary>

Не обязательно: репликация асинхронна, и последние записи могут пропасть.

</details>

**9. Подвох: проверка не уложилась в таймаут. Списан ли токен?**

<details><summary>Ответ</summary>

Неизвестно: команда может всё же выполниться.

</details>

**10. Где ставить ограничитель?**

<details><summary>Ответ</summary>

На шлюзе — грубые лимиты, в сервисе — бизнес-квоты.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [12 · Консенсус и координация](../../modules/12-consensus-and-coordination/overview.ru.md)

## Источники

- [Redis: scripting with Lua](https://redis.io/docs/latest/develop/programmability/eval-intro/)
- [Redis: Lua API](https://redis.io/docs/latest/develop/programmability/lua-api/)
- [Redis: TIME](https://redis.io/docs/latest/commands/time/)
- [RFC 6585, status 429](https://www.rfc-editor.org/rfc/rfc6585#section-4)
- [ASP.NET Core rate limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
