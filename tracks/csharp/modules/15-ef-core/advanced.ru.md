---
title: "EF Core: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 15-ef-core
part: 2
tags: [ef-core, n-plus-one, eager-loading, lazy-loading, split-query, no-tracking, optimistic-concurrency, transactions, query-filters, raw-sql, sql-injection]
versions: "EF Core 11.0, .NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# EF Core: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

## Загрузка связанных данных и проблема N+1

Тестовые данные: 3 блога, у каждого 2 поста. Нужно посчитать все посты. Количество SQL-команд считает перехватчик `CommandLog`.

| Способ | Код | Команд SQL |
|---|---|---|
| Ленивая загрузка | `blog.Posts.Count` в цикле | **4** (1 + 3) |
| Жадная загрузка | `Include(b => b.Posts)` | 1 |
| Проекция | `Select(b => new { b.Name, Posts = b.Posts.Count })` | 1 |
| Явная загрузка | `Entry(blog).Collection(b => b.Posts).Load()` | по команде на вызов |

**N+1** — один запрос за списком и ещё по одному на каждый элемент. При ленивой загрузке он появляется незаметно:

```csharp
// N+1: one query for blogs, then one more query per blog when Posts is touched.
public static (int TotalPosts, int Commands) LazyLoading(TestDatabase db)
{
    var log = new CommandLog();
    using BloggingContext context = db.CreateLazyContext(log);
    int total = 0;
    foreach (Blog blog in context.Blogs.ToList())
    {
        total += blog.Posts.Count; // each access to Posts runs a separate SELECT
    }
    return (total, log.Count);
}
```

Ленивая загрузка выключена по умолчанию. Для неё нужен пакет `Microsoft.EntityFrameworkCore.Proxies`, вызов `UseLazyLoadingProxies()` и `virtual`-навигации. Для 3 блогов это 4 запроса, для 1000 блогов — 1001.

Лечение — запросить всё сразу. **Проекция** обычно лучший вариант: база возвращает только нужные данные, и не нужно отслеживать сущности:

```csharp
var rows = context.Blogs
    .OrderBy(b => b.Name)
    .Select(b => new { b.Name, Posts = b.Posts.Count })
    .AsEnumerable()
    .Select(x => (x.Name, x.Posts))
    .ToList();
```

Результат `[("Blog 1", 2), ("Blog 2", 2), ("Blog 3", 2)]` — за 1 SQL-команду с подзапросом `COUNT(*)`.

Ещё два инструмента:

- **Фильтрованный `Include`**: `Include(b => b.Posts.Where(p => p.Likes >= 20))` загружает только часть коллекции. Для трёх блогов получилось `[0, 2, 2]` поста.
- **Явная загрузка**: `context.Entry(blog).Collection(b => b.Posts).Load()` подгружает навигацию по требованию. `IsLoaded` до вызова — `false`, после — `true`.

Код: [Ex04_Loading.cs](code/Examples/Ex04_Loading.cs).

### Split-запросы и декартов взрыв

Если одним запросом загрузить несколько коллекций, база вернёт декартово произведение: каждая строка одной коллекции соединяется с каждой строкой другой. По умолчанию EF Core использует один запрос (`SingleQuery`) и предупреждает об этом:

```text
warn: RelationalEventId.MultipleCollectionIncludeWarning[20504] (Microsoft.EntityFrameworkCore.Query)
      Compiling a query which loads related collections for more than one collection navigation, either via 'Include' or through projection, but no 'QuerySplittingBehavior' has been configured. By default, Entity Framework will use 'QuerySplittingBehavior.SingleQuery', which can potentially result in slow query performance.
```

`AsSplitQuery()` разбивает загрузку на отдельные запросы. Запрос с `Include(p => p.Tags)` и `Include(p => p.Blog).ThenInclude(b => b.Posts)` выполнился за 1 команду в режиме single и за 3 команды в режиме split. Глобально режим задаётся так: `UseSqlite(…, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))`. Цена split-запросов: несколько обращений к базе и нет гарантии согласованности между ними, если данные меняются параллельно.

## Трекинг: AsNoTracking и identity resolution

| Режим | Отслеживание | Один объект на строку | Когда |
|---|---|---|---|
| по умолчанию | да | да | данные будут изменены и сохранены |
| `AsNoTracking()` | нет | **нет** | чтение, максимальная скорость |
| `AsNoTrackingWithIdentityResolution()` | нет | да | чтение графа с повторяющимися объектами |

Шесть постов с `Include(p => p.Blog)`: с `AsNoTracking` получилось **6** разных объектов `Blog` (для каждого поста свой), с identity resolution — **3**.

**Отсоединённые сущности.** Объект пришёл из API и не загружался в этот контекст. `Update(entity)` помечает изменёнными **все** свойства, и EF обновляет все столбцы:

```text
UPDATE "Blogs" SET "IsDeleted" = @p0, "Name" = @p1, "Version" = @p2
WHERE "Id" = @p3 AND "Version" = @p4
RETURNING 1;
```

`Attach(entity)` подключает объект в состоянии `Unchanged`. После изменения свойства изменённым помечается только оно — проверено: `Name` — `IsModified = True`, `Version` — `False`.

## Что EF умеет перевести в SQL

EF переводит в SQL выражения, для которых знает перевод: операторы, многие методы `string`, `Math`, `DateTime`, агрегаты. Произвольный C#-метод перевести нельзя. Правило: **клиентская оценка разрешена только в последнем `Select`**:

```csharp
// Allowed: the untranslatable call is in the top-level Select, EF runs it on the loaded rows.
public static List<string> ClientEvalInProjection(BloggingContext context) =>
    context.Posts.OrderBy(p => p.Id).Select(p => Shorten(p.Title)).ToList();

// Not allowed: the same call in Where cannot be executed by the database, EF throws.
public static List<Post> ClientEvalInWhere(BloggingContext context) =>
    context.Posts.Where(p => Shorten(p.Title).EndsWith("…")).ToList();
```

Первый вызов возвращает `"Post 1…"` и другие сокращённые заголовки. Второй бросает `InvalidOperationException`: «The LINQ expression … could not be translated… Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'».

### IQueryable и IEnumerable

- `IQueryable<T>` — описание запроса. `Where` становится частью SQL: `WHERE … "p"."Likes" > 30`.
- `IEnumerable<T>` — уже LINQ to Objects. После `AsEnumerable()` фильтр работает в памяти: в примере, чтобы найти **1** пост, из базы прочитаны **все 6**.

Ошибка «поставил `AsEnumerable()` или `ToList()` слишком рано» превращает лёгкий запрос в выгрузку всей таблицы. Код: [Ex05_Translation.cs](code/Examples/Ex05_Translation.cs).

## ExecuteUpdate и ExecuteDelete

Классический путь «загрузить → изменить → `SaveChanges`» делает один `SELECT` и по `UPDATE` на сущность. Для массовых изменений есть операции, которые сразу выполняют один SQL-оператор:

```csharp
// UPDATE ... SET Likes = Likes + 1 WHERE ...: runs immediately, without SaveChanges.
public static (int Affected, int Commands, string Sql) LikeAllPostsOfBlog(BloggingContext context, CommandLog log, int blogId)
{
    log.Clear();
    int affected = context.Posts
        .Where(p => p.BlogId == blogId)
        .ExecuteUpdate(setters => setters.SetProperty(p => p.Likes, p => p.Likes + 1));
    return (affected, log.Count, log.Commands.Single());
}
```

Результат — 2 строки за 1 команду `UPDATE "Posts" AS "p" SET "Likes" = "p"."Likes" + 1 …`. Важные свойства:

- они **не используют change tracker**: уже загруженный объект остаётся устаревшим. Проверено: отслеживаемый пост показывает `Likes = 10`, в базе — `1000`;
- выполняются **сразу**, без `SaveChanges`;
- сами не открывают транзакцию.

`ExecuteDelete()` удаляет строки по условию и возвращает их число. Код: [Ex06_BulkOperations.cs](code/Examples/Ex06_BulkOperations.cs).

## Оптимистическая конкурентность

Два пользователя загрузили один блог и оба его меняют. Без защиты второй молча затрёт изменения первого — **потерянное обновление** (lost update). Оптимистическая блокировка: помечаем свойство как **токен конкурентности**, и EF добавляет его исходное значение в `WHERE`:

```text
UPDATE "Blogs" SET "Name" = @p0, "Version" = @p1
WHERE "Id" = @p2 AND "Version" = @p3
```

Если строку уже изменили, `UPDATE` затрагивает 0 строк, и EF бросает `DbUpdateConcurrencyException`: «The database operation was expected to affect 1 row(s), but actually affected 0 row(s); data may have been modified or deleted since entities were loaded».

| Вариант | Токен | Кто меняет значение |
|---|---|---|
| `[ConcurrencyCheck]` / `IsConcurrencyToken()` | любое свойство | приложение (в примере — `Version++`) |
| `[Timestamp]` / `IsRowVersion()` | `rowversion` | база (SQL Server) |

Разрешение конфликта:

```csharp
// "Database wins": throw away local changes and take the current values.
public static string DatabaseWins(BloggingContext context, DbUpdateConcurrencyException conflict)
{
    var entry = conflict.Entries.Single();
    entry.Reload();
    return ((Blog)entry.Entity).Name;
}
```

«Клиент побеждает»: взять значения из базы как новые исходные (`entry.OriginalValues.SetValues(entry.GetDatabaseValues())`) и сохранить ещё раз. Повтор с ограничением числа попыток — [упражнение 3](exercises.ru.md). Код: [Ex07_Concurrency.cs](code/Examples/Ex07_Concurrency.cs).

## Транзакции

- **Один `SaveChanges` атомарен:** если провайдер поддерживает транзакции, все изменения одного вызова применяются в транзакции. В примере второй блог нарушил уникальный индекс (`SQLite Error 19: 'UNIQUE constraint failed: Blogs.Name'`), и первый блог тоже не сохранился: `DbUpdateException`, в базе 0 записей.
- **Несколько `SaveChanges`** объединяют явной транзакцией:

```csharp
using (BloggingContext context = db.CreateContext())
using (var transaction = context.Database.BeginTransaction())
{
    context.Blogs.Add(new Blog { Name = "Tx 1" });
    context.SaveChanges();
    context.Blogs.Add(new Blog { Name = "Tx 2" });
    context.SaveChanges();
    transaction.Rollback();
}
```

После `Rollback` в базе 0 записей `Tx …`.

- **Точки сохранения.** Если транзакция уже открыта, `SaveChanges` сам создаёт savepoint. Неудачный `SaveChanges` откатывается только до него, и работа, сделанная раньше, сохраняется после `Commit`. Проверено: второй `SaveChanges` упал, первый блог остался.

Код: [Ex08_Transactions.cs](code/Examples/Ex08_Transactions.cs).

## Глобальные фильтры запросов

Фильтр из `OnModelCreating` автоматически добавляется к каждому запросу к типу. Типичные применения: мягкое удаление (soft delete) и мультиарендность (multi-tenancy). С EF Core 10 фильтры можно называть и отключать по имени:

```csharp
// Switch off one named filter for a single query (EF Core 10+).
public static int AllBlogsIncludingDeleted(BloggingContext context) =>
    context.Blogs.IgnoreQueryFilters([BloggingContext.SoftDeleteFilter]).Count();
```

После мягкого удаления одного блога видно 2 блога и 4 поста, а с `IgnoreQueryFilters` — все 3 блога. SQL запроса к блогам: `SELECT … FROM "Blogs" AS "b" WHERE NOT ("b"."IsDeleted")`.

Тонкость: фильтр на `Blog` не скрывает его посты. Запрос к `Posts` вернул бы посты удалённого блога. Поэтому в модели есть второй фильтр — `p => !p.Blog.IsDeleted`. Его цена — `JOIN` с `Blogs` в каждом запросе к постам (см. SQL в [базовой теории](basics.ru.md)). Код: [Ex09_QueryFilters.cs](code/Examples/Ex09_QueryFilters.cs).

## Сырой SQL

```csharp
// Safe: the interpolated value becomes a SQL parameter, not part of the SQL text.
public static int FindByNameSafe(BloggingContext context, string name) =>
    context.Blogs.FromSql($"SELECT * FROM Blogs WHERE Name = {name}").Count();

// Dangerous: the value is glued into the SQL text, so input can change the query itself.
#pragma warning disable EF1003 // the analyzer warns about exactly this mistake
public static int FindByNameUnsafe(BloggingContext context, string name) =>
    context.Blogs.FromSqlRaw("SELECT * FROM Blogs WHERE Name = '" + name + "'").Count();
#pragma warning restore EF1003
```

С вводом `x' OR '1'='1` безопасный вариант находит **0** блогов, а уязвимый — **все 3**: ввод изменил сам запрос. `FromSql` с интерполяцией превращает значение в параметр (`WHERE Name = @p0`), а для конкатенации внутри `FromSqlRaw` компилятор выдаёт предупреждение анализатора EF1003. Результат `FromSql` можно дальше компоновать LINQ: глобальный фильтр добавился поверх (`WHERE NOT ("e"."IsDeleted")`).

Для скаляров и типов вне модели — `Database.SqlQuery<T>`: `SqlQuery<int>($"SELECT SUM(Likes) AS Value FROM Posts").Single()` → `135`. Код: [Ex10_RawSql.cs](code/Examples/Ex10_RawSql.cs).

## Жизненный цикл DbContext

- `DbContext` рассчитан на **одну единицу работы** (unit of work): создать → выполнить операцию → `SaveChanges` → освободить. В ASP.NET Core `AddDbContext` регистрирует его как scoped — один на HTTP-запрос.
- `DbContext` **не потокобезопасен**. Параллельная операция на одном контексте бросает `InvalidOperationException`: «A second operation was started on this context instance before a previous operation completed». Каждый поток или каждая параллельная задача берёт свой контекст (см. [18 · Многопоточность](../18-concurrency/overview.ru.md)).
- Долгоживущий контекст накапливает отслеживаемые объекты: растёт память, и `SaveChanges` замедляется.

## Лучшие практики

- Чтение — проекции (`Select` в DTO) и `AsNoTracking`.
- Загружай связанные данные явно (`Include`, проекция); ленивую загрузку включай осознанно.
- Массовые изменения — `ExecuteUpdate`/`ExecuteDelete`.
- Защищай от потерянных обновлений токеном конкурентности.
- Смотри генерируемый SQL: `ToQueryString()`, `LogTo(Console.WriteLine)`.
- Асинхронные методы (`ToListAsync`, `SaveChangesAsync`) — в веб-приложениях, чтобы не блокировать потоки пула.
- Один контекст — одна единица работы.

## Частые ошибки

- ❌ Ленивая загрузка в цикле → ✅ N+1; `Include` или проекция.
- ❌ `ToList()`/`AsEnumerable()` до `Where` → ✅ фильтр выполнится в памяти после выгрузки всей таблицы.
- ❌ Несколько коллекций в одном `Include` на больших данных → ✅ `AsSplitQuery()`.
- ❌ `Update()` для частичного изменения → ✅ загрузить и изменить или `Attach` + изменение нужных свойств.
- ❌ `ExecuteUpdate`, а потом работа с загруженными сущностями → ✅ они устарели; перечитай.
- ❌ Конкатенация в `FromSqlRaw` → ✅ `FromSql` с интерполяцией.
- ❌ Общий `DbContext` между потоками или параллельными задачами → ✅ контекст на единицу работы.

## Сравнение с другими треками

| | C# — EF Core | Python — SQLAlchemy |
|---|---|---|
| Сеанс работы | `DbContext` | `Session` |
| Запросы | LINQ | `select()` |
| Миграции | `dotnet ef migrations` | Alembic |
| Ленивая загрузка по умолчанию | выключена | включена (`lazy="select"`) |

Подробнее — в [треке Python](../../../python/overview.ru.md), модуль 20 «Работа с БД».

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — устройство трекинга, наследование, маппинг, производительность, справочник, вопросы с собеседований
- Потоки и `DbContext`: [18 · Многопоточность](../18-concurrency/advanced.ru.md)
- `IDisposable` и `using`: [12 · Память и GC](../12-memory-and-gc/basics.ru.md#idisposable-и-using)

## Источники

- [Загрузка связанных данных](https://learn.microsoft.com/ef/core/querying/related-data/)
- [Ленивая загрузка](https://learn.microsoft.com/ef/core/querying/related-data/lazy)
- [Single и split-запросы](https://learn.microsoft.com/ef/core/querying/single-split-queries)
- [Клиентская и серверная оценка](https://learn.microsoft.com/ef/core/querying/client-eval)
- [ExecuteUpdate и ExecuteDelete](https://learn.microsoft.com/ef/core/saving/execute-insert-update-delete)
- [Конфликты конкурентности](https://learn.microsoft.com/ef/core/saving/concurrency)
- [Транзакции](https://learn.microsoft.com/ef/core/saving/transactions)
- [Глобальные фильтры запросов](https://learn.microsoft.com/ef/core/querying/filters)
- [SQL-запросы](https://learn.microsoft.com/ef/core/querying/sql-queries)
- [Время жизни и настройка DbContext](https://learn.microsoft.com/ef/core/dbcontext-configuration/)
