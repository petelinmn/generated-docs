---
title: "Dapper: продвинутый уровень"
lang: ru
track: csharp
kind: library
doc: advanced
slug: dapper
part: 2
tags: [dapper, multi-mapping, transactions, type-mapping, query-multiple]
versions: ".NET 11, C# 15; Dapper 2.1.100, Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Dapper: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

## Сопоставление имён

Dapper сопоставляет столбцы и свойства по имени. Столбец `created_at` не попадёт в свойство `CreatedAt`. Проверено: без настройки `CreatedAt = null`. Включите сопоставление с подчёркиваниями:

```csharp
DefaultTypeMap.MatchNamesWithUnderscores = true;
```

> **Подвох:** настройка действует на типы, которые Dapper ещё **не** сопоставлял. Проверено: для уже использованного типа `UserCreated` после включения всё равно `null`, а для нового типа `UserCreated2` — `2026-01-01`. Включайте настройку один раз на старте, до первого запроса.

Альтернативы: алиасы в SQL (`created_at AS CreatedAt`) или собственный `SqlMapper.SetTypeMap`.

## Список в условии `IN`

```csharp
cn.Query<User>("SELECT Id, Name FROM Users WHERE Id IN @ids", new { ids = new[] { 1, 3 } });
```

Проверено: вернулись `Ann,Cid`. Dapper разворачивает коллекцию в список параметров.

## Связанные таблицы (multi-mapping)

```csharp
var joined = cn.Query<Order, User, Order>(
    "SELECT o.Id, o.Total, u.Id, u.Name FROM Orders o JOIN Users u ON u.Id = o.UserId",
    (o, u) => { o.User = u; return o; },
    splitOn: "Id");
```

Проверено: `1:Ann,2:Ann,3:Bob`. `splitOn` указывает столбец, с которого начинается второй объект (по умолчанию `Id`). Для связи «один ко многим» результат нужно группировать вручную (`GroupBy`), иначе родитель дублируется.

## Несколько результатов за один запрос

```csharp
using var multi = cn.QueryMultiple("SELECT COUNT(*) FROM Users; SELECT Name FROM Users ORDER BY Id");
var c = multi.ReadSingle<long>(); var names = multi.Read<string>().ToList();
```

Проверено: `3 / Ann,Bob,Cid`. Это экономит обращения к БД.

## Транзакции

```csharp
using var tx = cn.BeginTransaction();
cn.Execute("INSERT INTO Users(Name) VALUES ('Tmp')", transaction: tx);
tx.Rollback();
```

Проверено: после `Rollback` число строк осталось 3. Транзакцию нужно передавать в каждый вызов (`transaction: tx`) — Dapper сам её не подхватывает.

## Хранимые процедуры и динамические параметры

`cn.Query<T>("sp_name", new { … }, commandType: CommandType.StoredProcedure)`; для выходных параметров — `DynamicParameters`.

> ⚠️ Проверить: хранимые процедуры и `DynamicParameters` с выходными параметрами требуют СУБД, поддерживающей их (SQLite не поддерживает) — в этом материале не запускались.

## Асинхронность

Каждый метод имеет `…Async`-вариант: `QueryAsync<T>`, `ExecuteAsync`, `QueryFirstOrDefaultAsync<T>`. Передавайте `CommandDefinition` с `CancellationToken`.

> ⚠️ Проверить: сигнатуры `CommandDefinition` и отмены в этом материале не запускались.

## Best practices

| Правило | Почему |
|---|---|
| Только параметризованные запросы | безопасность |
| Соединение — через `using`, коротко живущее | пул соединений |
| Настройки маппинга — на старте | кэш типов |
| Явный список столбцов вместо `SELECT *` | стабильность и скорость |
| Транзакция передаётся явно | иначе вне транзакции |
| Асинхронные методы в веб-приложениях | не блокировать потоки |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md) — сравнение с полноценным ORM.
- [Testcontainers](../testcontainers/overview.ru.md) — тесты на настоящей БД.
- [19 · Производительность](../../modules/19-performance/overview.ru.md) — измерения.

## Источники

- [Dapper README](https://github.com/DapperLib/Dapper#readme)
- [Dapper: multi-mapping](https://github.com/DapperLib/Dapper#multi-mapping)
