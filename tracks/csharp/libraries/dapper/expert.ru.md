---
title: "Dapper: экспертный уровень"
lang: ru
track: csharp
kind: library
doc: expert
slug: dapper
part: 2
tags: [dapper, ef-core-comparison, il-emit, caching, interview-questions]
versions: ".NET 11, C# 15; Dapper 2.1.100"
status: draft
updated: 2026-11-22
---

# Dapper: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md) и [продвинутый](advanced.ru.md). **Время:** ~30 мин.

## Внутреннее устройство

Dapper при первом выполнении запроса генерирует через IL-эмит (`System.Reflection.Emit`) метод разбора `IDataReader` в объект и кэширует его по «идентичности» запроса (SQL, тип параметров, тип результата, строка соединения). Повторные вызовы с тем же SQL используют готовый десериализатор — поэтому Dapper близок по скорости к ручному коду на `DbDataReader`.

Следствие, которое проверено: настройки сопоставления имён влияют только на ещё не закэшированные типы ([продвинутый](advanced.ru.md#сопоставление-имён)).

> ⚠️ Проверить: детали кэша (размер, вытеснение) и поведение IL-эмита под Native AOT в этом материале не исследовались — для AOT смотрите документацию Dapper (`Dapper.AOT`).

## Dapper или EF Core

| | Dapper | EF Core |
|---|---|---|
| SQL | вручную | генерируется из LINQ |
| Трекинг и `SaveChanges` | нет | есть |
| Миграции | нет | есть |
| Скорость на простых запросах | близка к ADO.NET | чуть ниже, зависит от настроек |
| Сложные отчёты, специфичный SQL | удобно | возможно через raw SQL |
| Смена СУБД | переписывать SQL | в основном провайдер |

Они хорошо сочетаются: записи и модель — в EF Core, тяжёлые отчёты — Dapper на том же соединении.

## Грани и подвохи

**1. Нет трекинга.** Изменили объект — в БД ничего не ушло, пока вы сами не выполнили `UPDATE`.

**2. Кэш сопоставления имён.** Проверено: `MatchNamesWithUnderscores` после первого использования типа не влияет на него.

**3. `QuerySingle` строг.** Проверено: 0 строк или больше одной — `InvalidOperationException`; `QueryFirstOrDefault` на 0 строк даёт `null`.

**4. Конкатенация SQL.** Проверено: инъекция `x' OR '1'='1` вернула все строки; параметр — 0.

**5. Multi-mapping и `splitOn`.** Неверный `splitOn` даёт пустые вложенные объекты без ошибки.

**6. Один ко многим.** Без группировки родитель повторяется на каждую дочернюю строку.

**7. Транзакция не неявная.** Если не передать `transaction:`, команда выполнится вне неё (для некоторых провайдеров — исключение).

**8. Буферизация.** `Query<T>` по умолчанию буферизует результат; для больших выборок — `buffered: false` и потоковая обработка при открытом соединении.

**9. Значения `NULL` и типы значений.** `NULL` в `int` (не `int?`) вызовет ошибку приведения — объявляйте nullable.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Dapper | скорость, контроль, простота | SQL вручную, нет миграций |
| EF Core | продуктивность, миграции, трекинг | абстракция «течёт», накладные расходы |
| Чистый ADO.NET | максимум контроля | много шаблонного кода |
| RepoDB, linq2db | другие микро-ORM/провайдеры | меньше сообщества |
| Source-generated маппинг (Dapper.AOT) | AOT-совместимость | отдельная настройка |

## Вопросы с собеседований

<details><summary>1. Что такое Dapper и чем он отличается от EF Core?</summary>

Микро-ORM: методы расширения над `IDbConnection`, SQL пишется вручную, маппинг в объекты автоматический; нет трекинга, миграций и LINQ.

</details>

<details><summary>2. Почему Dapper быстрый?</summary>

Он генерирует и кэширует код разбора `IDataReader` в объект (IL-эмит) и не добавляет слоёв трекинга.

</details>

<details><summary>3. Как защититься от SQL-инъекций в Dapper?</summary>

Параметры (`@name` и анонимный объект). Проверено: параметр — 0 строк, конкатенация — все строки.

</details>

<details><summary>4. Как сопоставить `created_at` со свойством `CreatedAt`?</summary>

`DefaultTypeMap.MatchNamesWithUnderscores = true` до первого использования типа, алиас в SQL или собственная карта типов.

</details>

<details><summary>5. Как загрузить связанные сущности?</summary>

Multi-mapping: `Query<A, B, A>(sql, map, splitOn: …)`; для «один ко многим» — группировка.

</details>

<details><summary>6. В чём разница `QueryFirst` и `QuerySingle`?</summary>

`First` берёт первую строку, `Single` требует ровно одну (проверено: иначе `InvalidOperationException`).

</details>

<details><summary>7. Как работать с транзакциями?</summary>

`BeginTransaction()` и передача `transaction:` в каждый вызов; `Commit/Rollback` (проверено: после `Rollback` строк не прибавилось).

</details>

<details><summary>8. Как передать список в `IN`?</summary>

Параметр-коллекция: `WHERE Id IN @ids` (проверено).

</details>

<details><summary>9. Подвох: включили `MatchNamesWithUnderscores`, но `CreatedAt` по-прежнему `null`. Почему?</summary>

Тип уже был сопоставлен и закэширован до включения настройки (проверено); включайте на старте приложения.

</details>

<details><summary>10. Подвох: запрос вернул родителя несколько раз при multi-mapping. Почему?</summary>

Связь «один ко многим» в JOIN даёт строку на каждого ребёнка; нужно сгруппировать по ключу родителя.

</details>

<details><summary>11. Когда выбрать Dapper, а когда EF Core?</summary>

Dapper — отчёты, горячие пути, сложный SQL, существующая БД; EF Core — CRUD с моделью, миграции, трекинг. Часто — вместе.

</details>

## Связанные материалы

- Назад: [продвинутый](advanced.ru.md). Проверьте себя: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md)
- [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md) — IL-эмит и генераторы.
- [BenchmarkDotNet](../benchmarkdotnet/overview.ru.md) — как измерять.

## Источники

- [Dapper README](https://github.com/DapperLib/Dapper#readme)
- [Dapper.AOT](https://github.com/DapperLib/DapperAOT)
