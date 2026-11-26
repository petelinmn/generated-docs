---
title: "Кэширование: экспертная теория"
lang: ru
track: system-design
kind: module
doc: expert
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: draft
updated: 2026-11-26
---

# Кэширование: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Даже «удаление после записи» оставляет окно гонки: читатель получил промах и прочитал старое значение, затем писатель обновил базу и удалил ключ, и только после этого читатель положил в кэш старое значение.

```mermaid
sequenceDiagram
    participant R as Reader
    participant C as Cache
    participant D as Database
    participant W as Writer
    R->>C: get (miss)
    R->>D: read v1
    W->>D: write v2
    W->>C: delete
    R->>C: set v1 (stale)
```

Окно закрывают версиями (старое не перезаписывает новое), арендой при промахе или повторным удалением с задержкой; TTL ограничивает ущерб в любом случае.

Доля попаданий нелинейно влияет на источник: при 10 000 запросах в секунду переход с 99% на 98% удваивает нагрузку на базу со 100 до 200.

## Справочник

| `HybridCache` | Значение |
|---|---|
| `MaximumPayloadBytes` | 1 МБ по умолчанию; большее не кэшируется и логируется |
| `MaximumKeyLength` | 1024 символа по умолчанию |
| `Expiration` | общий срок жизни записи |
| `LocalCacheExpiration` | срок жизни в L1 |
| Защита от stampede | в пределах одного экземпляра `HybridCache` |
| `RemoveByTagAsync("*")` | делает недействительными все данные |
| Объекты | по умолчанию каждый вызывающий получает отдельную копию; повторное использование — для `sealed` типов с `[ImmutableObject(true)]` |

| Политика `maxmemory-policy` в Redis | Что вытесняет |
|---|---|
| `noeviction` | ничего: записи, требующие памяти, получают ошибку |
| `allkeys-lru`, `allkeys-lfu`, `allkeys-random` | из всех ключей |
| `volatile-lru`, `volatile-lfu`, `volatile-random` | только из ключей с TTL |
| `volatile-ttl` | ключи с наименьшим остатком TTL |

LRU в Redis приближённый: сравнивается выборка ключей, а не все. Набор политик зависит от версии Redis; Redis в этом материале не запускался.

| Проблема | Признак | Средство |
|---|---|---|
| Stampede | всплеск запросов к базе при истечении ключа | single-flight, jitter, stale |
| Penetration | запросы несуществующих ключей | негативный кэш, фильтр Блума |
| Avalanche | многие ключи или весь кэш пропадают разом | jitter, резерв, ограничение нагрузки |
| Hot key | один узел перегружен | L1, копии ключа |

## Подвохи

### GetOrAdd может вызвать фабрику дважды

```csharp
var d = new ConcurrentDictionary<int, int>();
int calls = 0;
using var barrier = new Barrier(2);
var threads = Enumerable.Range(0, 2).Select(_ => new Thread(() =>
    d.GetOrAdd(1, k => { Interlocked.Increment(ref calls); barrier.SignalAndWait(); return 1; }))).ToArray();
```

После запуска и ожидания потоков: `calls` = 2, записей — 1. Фабрика выполняется вне блокировки. Поэтому значением делают `Lazy<Task<T>>`.

### IMemoryCache не защищает от stampede

```csharp
var callers = Enumerable.Range(0, 10)
    .Select(_ => mc.GetOrCreateAsync("k", e => { loads++; return gate.Task; })).ToArray();
```

Результат: `loads` = 10. `HybridCache` в той же ситуации делает одну загрузку.

### Кэш в памяти возвращает ту же ссылку

```csharp
mc.Set("list", new List<int> { 1 });
mc.Get<List<int>>("list")!.Add(2);
Console.WriteLine(mc.Get<List<int>>("list")!.Count);
```

Результат: `2` — вызывающий испортил кэш. Тот же опыт с `HybridCache` даёт 1 и разные экземпляры.

### SizeLimit требует Size

```csharp
using var limited = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1 });
limited.Set("a", 1);
```

Результат: `InvalidOperationException` — `Cache entry must specify a value for Size when SizeLimit is set.` Если задать размер 1 двум записям по очереди, в кэше остаётся первая: вторая не принимается, а не вытесняет её.

### null кэшируется, исключение — нет

Три вызова `HybridCache.GetOrCreateAsync<string?>` с фабрикой, возвращающей `null`, выполнили её один раз. Два вызова с фабрикой, бросающей исключение, выполнили её дважды.

### LRU и циклический доступ

`LruCache` ёмкостью 3 при обращениях по кругу к 4 ключам дал 0 попаданий из 400. Кэш ёмкостью 100 после прохода по 100 новым ключам не сохранил ни одного прежнего.

## Компромиссы и альтернативы

| Вариант | Плюс | Минус |
|---|---|---|
| Только L1 | минимальная задержка | расхождение и холодный старт на каждом экземпляре |
| Только L2 | одна копия | сеть и сериализация |
| L1 + L2 | баланс | сложнее инвалидация |
| Материализованное представление | согласованность задаётся обновлением | стоимость записи |
| Реплики чтения | свежее кэша, полный SQL | лаг репликации, дороже |

Иногда лучший кэш — индекс: сначала проверьте план запроса.

## Вопросы с собеседований

**1. Что такое cache-aside?**

<details><summary>Ответ</summary>

Приложение читает кэш, при промахе — источник и заполняет кэш; после записи инвалидирует.

</details>

**2. Почему удалять, а не обновлять кэш при записи?**

<details><summary>Ответ</summary>

Два писателя могут обновить кэш в порядке, обратном порядку записи в базу, и старое значение останется.

</details>

**3. Удаление после записи полностью устраняет гонки?**

<details><summary>Ответ</summary>

Нет: медленный читатель может положить старое значение после удаления.

</details>

**4. Что такое cache stampede и как с ним бороться?**

<details><summary>Ответ</summary>

Массовые одновременные загрузки одного отсутствующего ключа. Single-flight, jitter, отдача устаревшего.

</details>

**5. Решает ли single-flight проблему в кластере?**

<details><summary>Ответ</summary>

Только частично: по одной загрузке на экземпляр. Для строгого объединения нужна распределённая координация, у которой своя цена.

</details>

**6. Как реализовать LRU за O(1)?**

<details><summary>Ответ</summary>

Хэш-таблица ключ → узел двусвязного списка.

</details>

**7. Чем `no-cache` отличается от `no-store`?**

<details><summary>Ответ</summary>

`no-cache` разрешает хранить, но требует проверки перед использованием; `no-store` запрещает хранить.

</details>

**8. Что произойдёт при отказе кэша с долей попаданий 99%?**

<details><summary>Ответ</summary>

Нагрузка на источник вырастет примерно в 100 раз. Нужны ограничение нагрузки и деградация.

</details>

**9. Инвалидирует ли `HybridCache.RemoveByTagAsync` память других серверов?**

<details><summary>Ответ</summary>

Нет: действует на текущий экземпляр и L2.

</details>

**10. Подвох: доля попаданий 99%. Кэш работает хорошо?**

<details><summary>Ответ</summary>

Не обязательно: он может отдавать устаревшее, а 1% промахов может приходиться на самые дорогие запросы. Смотрите нагрузку на источник и задержку.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [HybridCache в ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Redis: key eviction](https://redis.io/docs/latest/develop/reference/eviction/)
