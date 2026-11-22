---
title: "Dapper: основы"
lang: ru
track: csharp
kind: library
doc: basics
slug: dapper
part: 2
tags: [dapper, micro-orm, sql, ado-net, sqlite]
versions: ".NET 11, C# 15; Dapper 2.1.100, Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Dapper: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [15 · EF Core](../../modules/15-ef-core/overview.ru.md) (для сравнения), знание SQL. **Время:** ~30 мин.

## Что это и зачем

Dapper — микро-ORM: набор методов расширения над `IDbConnection`. Вы пишете SQL сами, Dapper подставляет параметры и превращает строки результата в объекты. Нет трекинга изменений, миграций и LINQ-провайдера — только быстрый и предсказуемый маппинг. Выбирают, когда нужен полный контроль над SQL или максимум скорости.

```bash
dotnet add package Dapper
dotnet add package Microsoft.Data.Sqlite   # any ADO.NET provider works
```

## Запросы и команды

```csharp
using var cn = new SqliteConnection("Data Source=:memory:");
cn.Open();
cn.Execute("CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Email TEXT)");

var n = cn.Execute("INSERT INTO Users(Name, Email) VALUES (@Name, @Email)",
    new[] { new { Name = "Ann", Email = "a@x" }, new { Name = "Bob", Email = "b@x" }, new { Name = "Cid", Email = (string?)null } });

var users = cn.Query<User>("SELECT Id, Name, Email FROM Users WHERE Id > @min ORDER BY Id", new { min = 1 }).ToList();
var count = cn.ExecuteScalar<long>("SELECT COUNT(*) FROM Users");
```

Проверено запуском: `Execute` с массивом параметров вставил 3 строки (`rows inserted: 3`); запрос вернул `2:Bob:b@x,3:Cid:null`; `COUNT` → `3`. Свойства объекта (`@Name`) становятся параметрами команды. `NULL` из БД превращается в `null` для `string?`.

| Метод | Назначение |
|---|---|
| `Query<T>` | список объектов |
| `QueryFirst/QueryFirstOrDefault<T>` | первая строка (исключение/`null`, если нет) |
| `QuerySingle/QuerySingleOrDefault<T>` | ровно одна строка |
| `Execute` | INSERT/UPDATE/DELETE, возвращает число затронутых строк |
| `ExecuteScalar<T>` | одно значение |
| `QueryMultiple` | несколько результирующих наборов |

Проверено: `QueryFirstOrDefault` на отсутствующей строке — `null`; `QuerySingle` при 0 строк и при нескольких строках — `InvalidOperationException`.

## Параметры: защита от SQL-инъекций

Всегда передавайте значения параметрами, а не склейкой строк. Проверено на значении `x' OR '1'='1`: с параметром запрос вернул 0 строк, а с конкатенацией — все 3 (инъекция сработала).

```csharp
cn.Query<User>("SELECT * FROM Users WHERE Name = @n", new { n = userInput });   // safe
```

## Частые ошибки новичка

- ❌ Строить SQL конкатенацией → ✅ параметры (`@name`).
- ❌ Ожидать, что `created_at` сам попадёт в `CreatedAt` → ✅ нужна настройка маппинга ([продвинутый](advanced.ru.md#сопоставление-имён)).
- ❌ `Query<T>` и забыть `ToList()` при последующем использовании соединения → ✅ материализуйте результат до закрытия соединения.
- ❌ Ожидать трекинга/автообновления как в EF → ✅ изменения пишете SQL-командами.
- ❌ Открывать соединение и не закрывать → ✅ `using`.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — маппинг имён, `IN`, соединения, транзакции.
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md) — полноценный ORM; когда выбирать что.
- [Пагинация и фильтрация](../../recipes/pagination-filtering/overview.ru.md) — запросы с `LIMIT/OFFSET`.
- [Python](../../../python/overview.ru.md) — работа с БД в Python.

## Источники

- [Dapper на GitHub](https://github.com/DapperLib/Dapper)
- [Dapper: параметры и запросы (README)](https://github.com/DapperLib/Dapper#readme)
- [Microsoft.Data.Sqlite](https://learn.microsoft.com/dotnet/standard/data/sqlite/)
