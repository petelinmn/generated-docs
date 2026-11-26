---
title: "EF Core"
lang: ru
track: csharp
kind: module
doc: overview
slug: 15-ef-core
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [ef-core, orm, dbcontext, linq, change-tracking, migrations, n-plus-one, optimistic-concurrency, transactions, query-filters, raw-sql, inheritance-mapping, interceptors]
versions: "EF Core 11.0, .NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# EF Core

🌐 **Русский** · [English](overview.en.md)

## Коротко

Entity Framework Core — ORM от Microsoft: классы C# описывают таблицы, LINQ-запросы превращаются в SQL, а изменения объектов сохраняются одним вызовом `SaveChanges`. EF Core работает с SQL Server, PostgreSQL, SQLite и другими базами через провайдеры и ведёт историю схемы миграциями. Он снимает рутину, но не отменяет SQL: чтобы приложение было быстрым и корректным, нужно понимать, какой SQL генерируется, что отслеживается и где теряются обновления. Именно это чаще всего спрашивают на собеседованиях.

## Ключевые идеи

- **`DbContext` — единица работы:** он выполняет запросы, отслеживает загруженные сущности и сохраняет изменения в транзакции. Контекст не потокобезопасен; в веб-приложении — один на запрос.
- **LINQ → SQL:** операторы над `IQueryable` выполняются в базе, SQL уходит только при перечислении. Ранний `AsEnumerable()` или `ToList()` выгружает всю таблицу.
- **Отслеживание изменений:** EF сравнивает сущности со снимком и обновляет только изменённые столбцы. Для чтения — `AsNoTracking()`.
- **Связанные данные загружаются явно:** `Include` или проекция. Ленивая загрузка в цикле даёт проблему N+1: для 3 блогов — 4 запроса вместо 1.
- **Оптимистическая конкурентность:** токен в `WHERE` превращает потерянное обновление в `DbUpdateConcurrencyException`.
- **Массовые операции** `ExecuteUpdate`/`ExecuteDelete` выполняют один SQL-оператор, но не обновляют трекер.
- **Сырой SQL безопасен только с параметрами:** `FromSql($"…")`; конкатенация в `FromSqlRaw` — SQL-инъекция.

## Минимальный пример

```csharp
// Nothing is sent to the database until the query is enumerated (ToList, First, Count…).
public static List<string> PopularTitles(BloggingContext context, int minLikes) =>
    context.Posts
        .Where(p => p.Likes >= minLikes)
        .OrderByDescending(p => p.Likes)
        .Select(p => p.Title)
        .ToList();
```

EF генерирует `SELECT "p"."Title" FROM "Posts" … WHERE … "p"."Likes" >= @minLikes ORDER BY "p"."Likes" DESC`: фильтр и сортировка выполняются в базе, а значение передаётся параметром. На тестовых данных результат — `["Post 3.2", "Post 3.1", "Post 2.2"]`.

## Когда применять, а когда нет

- ✅ Бизнес-приложения: CRUD, граф объектов, транзакционная логика.
- ✅ Когда схема развивается вместе с кодом — миграции.
- ✅ API на чтение — с проекциями, `AsNoTracking` и скомпилированными запросами.
- ❌ Массовая загрузка миллионов строк — `SqlBulkCopy`, `COPY`.
- ❌ Сложные отчёты, которые проще написать на SQL, — `FromSql`/`SqlQuery` или Dapper.
- ❌ Ленивая загрузка в веб-API — прячет N+1.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | модель и `DbContext`; LINQ → SQL; `SaveChanges`; состояния сущностей; `Include`; миграции |
| [2. Продвинутая теория](advanced.ru.md) | N+1 и способы загрузки; split-запросы; `AsNoTracking` и identity resolution; клиентская оценка; `ExecuteUpdate`/`ExecuteDelete`; оптимистическая конкурентность; транзакции; глобальные фильтры; сырой SQL; время жизни контекста |
| [3. Экспертная теория](expert.ru.md) | конвейер запроса и параметры; снимки и `DetectChanges`; перехватчики; ленивая загрузка изнутри; TPH/TPT/TPC; конвертеры, complex types, теневые свойства; скомпилированные запросы и пул; справочник, подвохи, 21 вопрос с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| [5. Запускаемые примеры](code/) | `code/Examples/` и `code/Solutions/` — решение `Code.slnx`, SQLite в памяти |
| [6. Тесты](code/) | `code/Tests/` — `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [02 · Типы и переменные](../02-types-and-variables/overview.ru.md); [07 · LINQ](../07-linq/overview.ru.md); [10 · Async/await](../10-async-await/overview.ru.md)
- `DbContext` и потоки: [18 · Многопоточность](../18-concurrency/overview.ru.md)
- Инструмент `dotnet ef`: [01 · Платформа .NET и dotnet CLI](../01-dotnet-platform/expert.ru.md)
- `required`/`init` в сущностях, первичные конструкторы: [11 · Современный C#](../11-modern-csharp/overview.ru.md)
- API поверх данных: [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md)
- Тесты с БД: [17 · Тестирование: интеграционные тесты](../17-testing-integration/overview.ru.md)
- Репозиторий и агрегаты: [21 · Архитектура приложений](../21-architecture/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — модуль 20 «Работа с БД»: SQLAlchemy и Alembic
- Рецепты и библиотеки: [Dapper](../../libraries/dapper/overview.ru.md), [Mapperly](../../libraries/mapperly/overview.ru.md), [Testcontainers](../../libraries/testcontainers/overview.ru.md), [Кэширование](../../recipes/caching/overview.ru.md), [Пагинация и фильтрация](../../recipes/pagination-filtering/overview.ru.md)
- [System design: выбор хранилища](../../../system-design/modules/04-data-storage/overview.ru.md)
- [System design: согласованность и CAP](../../../system-design/modules/07-consistency-and-cap/overview.ru.md)
- [System design: распределённые транзакции](../../../system-design/modules/10-distributed-transactions/overview.ru.md)
- [Индексы в PostgreSQL](../../../postgresql/modules/05-indexes/overview.ru.md)
- [Транзакции в PostgreSQL](../../../postgresql/modules/06-transactions/overview.ru.md)
- [Представления и функции в PostgreSQL](../../../postgresql/modules/07-views-functions/overview.ru.md)
