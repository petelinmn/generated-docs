---
title: "Dapper"
lang: ru
track: csharp
kind: library
doc: overview
slug: dapper
part: 2
levels: [1, 2, 3, 4]
tags: [dapper, micro-orm, sql, ado-net]
versions: ".NET 11, C# 15; Dapper 2.1.100, Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-22
---

# Dapper

🌐 **Русский** · [English](overview.en.md)

## Коротко

Dapper — микро-ORM: методы расширения над `IDbConnection`. Вы пишете SQL, Dapper подставляет параметры и маппит строки в объекты. Быстрый (кэширует сгенерированный разбор `IDataReader`), предсказуемый, без трекинга и миграций. Часто используют вместе с EF Core: модель и записи — EF, тяжёлые отчёты — Dapper.

## Ключевые идеи

- `Query<T>`, `QueryFirst/Single`, `Execute`, `ExecuteScalar`, `QueryMultiple`.
- Параметры — анонимным объектом; конкатенация SQL открывает инъекцию (проверено).
- Маппинг по имени; `created_at` требует настройки, действующей до первого использования типа.
- Multi-mapping с `splitOn`; «один ко многим» группируется вручную.
- Транзакция передаётся явно.
- Нет трекинга: `UPDATE` пишете сами.

## Минимальный пример

```csharp
using var cn = new SqliteConnection("Data Source=app.db");
var users = cn.Query<User>("SELECT Id, Name FROM Users WHERE Id > @min", new { min = 1 }).ToList();
```

## Когда применять, а когда нет

- ✅ Отчёты, горячие пути, сложный SQL.
- ✅ Существующая БД без модели EF.
- ✅ Нужна скорость и контроль.
- ❌ Много CRUD по богатой модели с изменениями (удобнее EF Core).
- ❌ Нужны миграции и трекинг «из коробки».
- ❌ Команда не готова поддерживать SQL вручную.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Основы](basics.ru.md) | запросы, параметры, методы, инъекции |
| [2. Продвинутый](advanced.ru.md) | маппинг имён, `IN`, multi-mapping, транзакции |
| [3. Эксперт](expert.ru.md) | устройство, сравнение с EF, подвохи, 11 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 10 вопросов и [4 упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | не сгенерирован — `/learn csharp dapper level 5` |
| 6. Тесты | не сгенерирован — `/learn csharp dapper level 6` |

## Связанные материалы

- Пререквизиты: [15 · EF Core](../../modules/15-ef-core/overview.ru.md)
- Рядом: [Пагинация и фильтрация](../../recipes/pagination-filtering/overview.ru.md), [Testcontainers](../testcontainers/overview.ru.md), [BenchmarkDotNet](../benchmarkdotnet/overview.ru.md)
- В других треках: [Python](../../../python/overview.ru.md) — работа с БД в Python
- Рецепты и библиотеки: [Mapperly](../mapperly/overview.ru.md)
- [System design: выбор хранилища](../../../system-design/modules/04-data-storage/overview.ru.md)
