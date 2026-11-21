---
title: "Упражнения: EF Core"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 15-ef-core
part: 2
tags: [ef-core, projection, n-plus-one, optimistic-concurrency, interceptors, soft-delete]
status: draft
updated: 2026-11-21
---

# Упражнения: EF Core

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами, запускаемые версии — в [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), тесты к ним — в [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).
>
> Модель и тестовая база — из [Ex01_Model.cs](code/Examples/Ex01_Model.cs): `TestDatabase` создаёт SQLite в памяти, `Seed()` добавляет 3 блога («Blog 1»…«Blog 3»), у блога `b` посты «Post b.1» с `b * 10` лайками и «Post b.2» с `b * 10 + 5`.

## 1. 🟢 Самые популярные посты

**Задача:** напиши `PostQueries.TopTitles(context, minLikes, take)`: заголовки постов, у которых не меньше `minLikes` лайков, по убыванию лайков (при равенстве — по заголовку), не больше `take` штук. Фильтр, сортировка и ограничение должны выполняться в базе. Запрос только читает данные.

**Ожидаемый результат:**

```text
TopTitles(context, minLikes: 20, take: 3) → ["Post 3.2", "Post 3.1", "Post 2.2"]
```

<details><summary>Подсказка</summary>

Все операторы — над `IQueryable`, `ToList()` в самом конце. Для чтения подойдёт `AsNoTracking()`, а `Select` вернёт только заголовки.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: a read-only query — filter, sort, limit and project in the database.
public static class PostQueries
{
    public static List<string> TopTitles(BloggingContext context, int minLikes, int take) =>
        context.Posts
            .AsNoTracking()
            .Where(p => p.Likes >= minLikes)
            .OrderByDescending(p => p.Likes)
            .ThenBy(p => p.Title)
            .Take(take)
            .Select(p => p.Title)
            .ToList();
}
```

`ThenBy` делает порядок детерминированным при одинаковом числе лайков. Строго говоря, `AsNoTracking` здесь не обязателен: проекция в `string` не возвращает сущностей, и отслеживать нечего. Но он явно показывает намерение «только чтение».

</details>

## 2. 🟡 Отчёт без N+1

**Задача:** код ниже строит отчёт по блогам, но при ленивой загрузке выполняет 1 + N запросов. Напиши `BlogReports.Summaries(context)`, который возвращает тот же результат — `(Blog, Posts, TotalLikes)` по каждому блогу, отсортированный по имени, — **одним** SQL-запросом.

```csharp
foreach (Blog blog in context.Blogs.OrderBy(b => b.Name).ToList())
{
    rows.Add((blog.Name, blog.Posts.Count, blog.Posts.Sum(p => p.Likes)));
}
```

**Ожидаемый результат:**

```text
[("Blog 1", 2, 25), ("Blog 2", 2, 45), ("Blog 3", 2, 65)], SQL-команд: 1
```

<details><summary>Подсказка</summary>

Не загружай посты вовсе: попроси базу посчитать `Count` и `Sum` в проекции. Литерал кортежа внутри `IQueryable`-запроса не скомпилируется: ошибка CS8143 «дерево выражений не может содержать литерал кортежа». Спроецируй в анонимный тип, а в кортеж преобразуй после `AsEnumerable()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: replace an N+1 loop with one query that projects the aggregates.
public static class BlogReports
{
    public static List<(string Blog, int Posts, int TotalLikes)> Summaries(BloggingContext context) =>
        context.Blogs
            .OrderBy(b => b.Name)
            .Select(b => new { b.Name, Posts = b.Posts.Count, TotalLikes = b.Posts.Sum(p => p.Likes) })
            .AsEnumerable()
            .Select(x => (x.Name, x.Posts, x.TotalLikes))
            .ToList();
}
```

`Count` и `Sum` становятся подзапросами в одном `SELECT`, а база возвращает три короткие строки. `AsEnumerable()` стоит после проекции, поэтому в память приходят уже готовые агрегаты. Тест проверяет число команд перехватчиком `CommandLog`.

</details>

## 3. 🟡 Переименование с повтором при конфликте

**Задача:** напиши `BlogService.RenameWithRetry(context, blogId, newName, maxAttempts = 3)`. Метод переименовывает блог и увеличивает `Version`. Если кто-то изменил блог между загрузкой и сохранением, `SaveChanges` бросит `DbUpdateConcurrencyException` — тогда возьми из базы свежие исходные значения и повтори («клиент побеждает»). Верни номер успешной попытки. После `maxAttempts` неудач исключение должно выйти наружу.

**Ожидаемый результат:** тест перед первым сохранением переименовывает блог через другой контекст.

```text
RenameWithRetry(context, blogId: 1, "Mine") → 2, имя в базе: "Mine"
```

<details><summary>Подсказка</summary>

`conflict.Entries.Single()` — запись трекера для блога. `GetDatabaseValues()` вернёт текущие значения строки (или `null`, если её удалили). Скопируй их в `OriginalValues` — тогда следующий `UPDATE` будет сравнивать с актуальным `Version`. Ограничить число попыток удобно фильтром `catch … when (attempt < maxAttempts)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: optimistic concurrency with a retry — "client wins" on the name.
public static class BlogService
{
    public static int RenameWithRetry(BloggingContext context, int blogId, string newName, int maxAttempts = 3)
    {
        Blog blog = context.Blogs.Single(b => b.Id == blogId);
        for (int attempt = 1; ; attempt++)
        {
            blog.Name = newName;
            blog.Version++;
            try
            {
                context.SaveChanges();
                return attempt;
            }
            catch (DbUpdateConcurrencyException conflict) when (attempt < maxAttempts)
            {
                // Refresh original values (and the token) from the database, then try again.
                var entry = conflict.Entries.Single();
                var databaseValues = entry.GetDatabaseValues()
                    ?? throw new InvalidOperationException("The blog was deleted by someone else.");
                entry.OriginalValues.SetValues(databaseValues);
                blog.Version = databaseValues.GetValue<int>(nameof(Blog.Version));
            }
        }
    }
}
```

После `SetValues` исходное значение `Version` совпадает с базой, а текущее перед следующей попыткой снова увеличивается на 1. Когда попытки кончаются, фильтр `when` не срабатывает, и исключение выходит наружу. Если строку удалили, повторять бессмысленно — метод сообщает об этом явно.

</details>

## 4. 🔴 Мягкое удаление через перехватчик

**Задача:** напиши `SoftDeleteInterceptor : SaveChangesInterceptor`. Когда код вызывает `context.Blogs.Remove(blog)`, вместо `DELETE` должно выполниться `UPDATE`: `IsDeleted = true`. Посты блога остаются в базе, а глобальный фильтр скрывает блог из обычных запросов.

**Ожидаемый результат:**

```text
Remove(блог 1) + SaveChanges
check.Blogs.Count() → 2
check.Blogs.IgnoreQueryFilters().Single(b => b.Id == 1).IsDeleted → true
посты блога 1 в базе (IgnoreQueryFilters) → 2
```

<details><summary>Подсказка</summary>

Переопредели `SavingChanges`: он вызывается до того, как EF построит SQL. Найди в `ChangeTracker.Entries<Blog>()` записи в состоянии `Deleted` и переведи их в `Modified`. Не забудь про токен конкурентности.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: turn Remove() into a soft delete for blogs.
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            foreach (var entry in eventData.Context.ChangeTracker.Entries<Blog>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified; // UPDATE instead of DELETE
                    entry.Entity.IsDeleted = true;
                    entry.Entity.Version++;
                }
            }
        }
        return result;
    }
}
```

Посты не удаляются: блог загружен без постов, поэтому каскад в трекере их не затронул, а в базу ушёл `UPDATE`, а не `DELETE`, и каскад базы тоже не сработал. Если посты были бы загружены, `Remove` перевёл бы их в `Deleted` — тогда перехватчику пришлось бы обработать и их. Для `SaveChangesAsync` нужно переопределить ещё и `SavingChangesAsync`.

</details>
