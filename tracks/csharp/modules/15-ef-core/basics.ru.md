---
title: "EF Core: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 15-ef-core
part: 2
tags: [ef-core, orm, dbcontext, linq, change-tracking, migrations, sqlite]
versions: "EF Core 11.0, .NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# EF Core: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

**Пререквизиты:** [02 · Типы и переменные](../02-types-and-variables/overview.ru.md); [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md) и [07 · LINQ](../07-linq/overview.ru.md).

## Зачем это нужно

Приложение работает с объектами, а реляционная база — с таблицами и SQL. **ORM** (object-relational mapper) переводит одно в другое. Entity Framework Core — ORM от Microsoft:

- описывает таблицы классами C# (**модель**);
- превращает LINQ-запросы в SQL и результаты — обратно в объекты;
- отслеживает изменения объектов и сам генерирует `INSERT`, `UPDATE`, `DELETE`;
- ведёт историю изменений схемы — **миграции**.

С базой EF Core работает через **провайдер**: SQL Server, PostgreSQL (Npgsql), SQLite, MySQL и другие. Примеры этого модуля используют SQLite в памяти: это настоящая реляционная база, поэтому видно реальный SQL, но тесты не требуют сервера.

## Модель и DbContext

Модель — обычные классы. EF Core многое понимает по соглашениям (conventions): свойство `Id` — первичный ключ, `BlogId` — внешний ключ к `Blog`, `List<Post>` — навигация «один ко многим».

```csharp
// Navigations are virtual so that the same model also works with lazy-loading proxies.
public class Blog
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsDeleted { get; set; }

    // Application-managed concurrency token: every update must increment it.
    public int Version { get; set; }

    public virtual List<Post> Posts { get; set; } = [];
}

public class Post
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int Likes { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public int BlogId { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual List<Tag> Tags { get; set; } = [];
}
```

`DbContext` — сеанс работы с базой: он даёт `DbSet<T>` для запросов и хранит отслеживаемые объекты. То, что не выводится соглашениями, настраивают в `OnModelCreating` через **Fluent API**:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>(blog =>
    {
        blog.Property(b => b.Name).HasMaxLength(100);
        blog.HasIndex(b => b.Name).IsUnique();
        blog.Property(b => b.Version).IsConcurrencyToken();
        // A named query filter (EF Core 10+): can be switched off by name.
        blog.HasQueryFilter(SoftDeleteFilter, b => !b.IsDeleted);
    });
```

Связь «многие ко многим» `Post` ↔ `Tag` EF создаёт сам: в сгенерированной схеме появляется таблица `PostTag`:

```text
CREATE TABLE "PostTag" (
    "PostsId" INTEGER NOT NULL,
    "TagsId" INTEGER NOT NULL,
    CONSTRAINT "PK_PostTag" PRIMARY KEY ("PostsId", "TagsId"),
```

Контекст получает настройки (провайдер, строку подключения) через `DbContextOptions`: `new DbContextOptionsBuilder<BloggingContext>().UseSqlite(connection)`. Код: [Ex01_Model.cs](code/Examples/Ex01_Model.cs).

## Запросы: LINQ превращается в SQL

```csharp
// Nothing is sent to the database until the query is enumerated (ToList, First, Count…).
public static List<string> PopularTitles(BloggingContext context, int minLikes) =>
    context.Posts
        .Where(p => p.Likes >= minLikes)
        .OrderByDescending(p => p.Likes)
        .Select(p => p.Title)
        .ToList();
```

`ToQueryString()` показывает SQL, не выполняя его. Для этого запроса EF генерирует:

```text
.param set @minLikes 25
SELECT "p"."Title"
FROM "Posts" AS "p"
INNER JOIN (
    SELECT "b"."Id", "b"."IsDeleted"
    FROM "Blogs" AS "b"
    WHERE NOT ("b"."IsDeleted")
) AS "b0" ON "p"."BlogId" = "b0"."Id"
WHERE NOT ("b0"."IsDeleted") AND "p"."Likes" >= @minLikes
ORDER BY "p"."Likes" DESC
```

- `Where`, `OrderBy`, `Select` выполняются **в базе**: возвращаются только нужные строки и столбцы.
- Переменная `minLikes` стала **параметром** `@minLikes`, а не частью текста SQL — это защита от SQL-инъекций.
- `INNER JOIN` с `Blogs` добавил фильтр мягкого удаления из модели — он разобран в [продвинутой теории](advanced.ru.md).
- Запрос выполняется только при перечислении: `ToList()`, `First()`, `Count()`, `foreach`. До этого `IQueryable` — лишь описание запроса (отложенное выполнение).

Результат на тестовых данных: `["Post 3.2", "Post 3.1", "Post 2.2"]`. Код: [Ex02_Crud.cs](code/Examples/Ex02_Crud.cs).

## Сохранение: SaveChanges

```csharp
public static int CreateBlog(BloggingContext context, string name, params string[] postTitles)
{
    var blog = new Blog { Name = name };
    foreach (string title in postTitles)
    {
        blog.Posts.Add(new Post { Title = title });
    }
    context.Blogs.Add(blog);       // the whole graph becomes Added
    context.SaveChanges();         // one transaction; generated keys are written back
    return blog.Id;
}

// Load, change, save: the change tracker finds the modified property and updates only it.
public static void Rename(BloggingContext context, int blogId, string newName)
{
    Blog blog = context.Blogs.Single(b => b.Id == blogId);
    blog.Name = newName;
    blog.Version++;
    context.SaveChanges();
}
```

`Add` помечает для вставки весь граф объектов: блог и его посты. `SaveChanges` отправляет все изменения в базу одной транзакцией и записывает сгенерированные ключи обратно в объекты. При изменении EF обновляет **только изменённые столбцы**:

```text
UPDATE "Blogs" SET "Name" = @p0, "Version" = @p1
WHERE "Id" = @p2 AND "Version" = @p3
RETURNING 1;
```

`Remove` удаляет сущность. В примере вместе с блогом удаляются его посты: связь обязательная, и по умолчанию для неё настроено каскадное удаление. `SaveChanges` вернул `3`: 1 блог + 2 поста.

## Отслеживание изменений

Сущности, полученные запросом, **отслеживаются** (tracked): контекст хранит их исходные значения и при `SaveChanges` сравнивает с текущими.

| Состояние | Что значит | Что сделает `SaveChanges` |
|---|---|---|
| `Detached` | контекст не знает об объекте | ничего |
| `Added` | новый объект | `INSERT` |
| `Unchanged` | загружен и не менялся | ничего |
| `Modified` | изменены свойства | `UPDATE` |
| `Deleted` | помечен на удаление | `DELETE` |

Пример проводит один объект через все состояния. Реальный результат: `Detached → Added → Unchanged → Modified → Deleted → Detached`. Для перехода в `Modified` не нужен никакой вызов EF: достаточно присвоить свойство, контекст заметит изменение сам. Код: [Ex03_ChangeTracking.cs](code/Examples/Ex03_ChangeTracking.cs).

- **`AsNoTracking()`** — запрос без отслеживания. Он быстрее, но изменения таких объектов `SaveChanges` не сохранит (вернёт `0`). Подходит для чтения: отчёты, API на чтение.
- **`Find(id)`** сначала ищет сущность среди отслеживаемых и ходит в базу, только если не нашёл. Проверено: повторный `Find` уже загруженного блога выполнил `0` SQL-команд.

## Связанные данные: Include

По умолчанию навигации не загружаются: `blog.Posts` после обычного запроса пуст. Связанные данные запрашивают явно:

```csharp
context.Blogs.Include(b => b.Posts).ToList();
```

`Include` добавляет `JOIN`, и всё приходит одним запросом. Остальные способы — ленивая и явная загрузка, проекция, split-запросы — и главная ловушка «N+1» разобраны в [продвинутой теории](advanced.ru.md).

## Миграции

Миграции хранят историю изменений схемы в виде кода. Инструмент — `dotnet ef`, локальный .NET-инструмент (см. [01 · Платформа .NET](../01-dotnet-platform/expert.ru.md)). Проекту нужен пакет `Microsoft.EntityFrameworkCore.Design`.

```bash
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 11.0.0
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations script
```

Реальный результат:

- `migrations add` создал три файла: `20261121042657_InitialCreate.cs` (сама миграция), `20261121042657_InitialCreate.Designer.cs` (метаданные) и `AppContext2ModelSnapshot.cs` (снимок текущей модели — с ним сравнивается следующая миграция).
- `database update` применил миграцию и записал её в таблицу `__EFMigrationsHistory`.
- `migrations script` выдал SQL-скрипт для ручного применения.

Миграция — это класс с методами `Up` (применить) и `Down` (откатить):

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Blogs",
        columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(type: "TEXT", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Blogs", x => x.Id);
        });
}
```

`Database.EnsureCreated()` создаёт схему без миграций — удобно для тестов и прототипов (так сделаны примеры модуля). Смешивать его с миграциями нельзя: схема, созданная `EnsureCreated`, ломает последующий `Migrate`.

## Ошибки новичка

- ❌ Читать `blog.Posts` без `Include` и удивляться пустому списку → ✅ `Include` или проекция `Select`.
- ❌ Ожидать, что запрос выполнится в момент вызова `Where` → ✅ SQL уходит при `ToList`, `First`, `Count`, `foreach`.
- ❌ Склеивать SQL строками → ✅ LINQ или `FromSql` с интерполяцией: значения станут параметрами.
- ❌ Забыть `SaveChanges` → ✅ без него изменения остаются только в памяти.
- ❌ Отслеживающие запросы для чтения больших объёмов → ✅ `AsNoTracking()`.
- ❌ Менять схему базы руками → ✅ миграции: `dotnet ef migrations add`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — N+1, конкурентность, транзакции, фильтры, сырой SQL
- Инструменты .NET и `dotnet tool`: [01 · Платформа .NET и dotnet CLI](../01-dotnet-platform/expert.ru.md)
- Потокобезопасность `DbContext`: [18 · Многопоточность](../18-concurrency/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — модуль 20 «Работа с БД»: SQLAlchemy и Alembic

## Источники

- [Обзор EF Core](https://learn.microsoft.com/ef/core/)
- [Создание модели](https://learn.microsoft.com/ef/core/modeling/)
- [Запросы](https://learn.microsoft.com/ef/core/querying/)
- [Сохранение данных](https://learn.microsoft.com/ef/core/saving/)
- [Отслеживание изменений](https://learn.microsoft.com/ef/core/change-tracking/)
- [Миграции](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [Применение миграций](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying)
