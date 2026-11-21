---
title: "Квиз: EF Core"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 15-ef-core
part: 2
tags: [ef-core, change-tracking, n-plus-one, optimistic-concurrency, raw-sql, inheritance-mapping]
status: draft
updated: 2026-11-21
---

# Квиз: EF Core

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).
>
> Во всех вопросах данные одни и те же: 3 блога, у каждого 2 поста, ленивая загрузка выключена, если не сказано иначе.

## 1. 🟢 На какой строке EF отправит SQL в базу?

```csharp
IQueryable<Post> query = context.Posts.Where(p => p.Likes > 10);   // 1
query = query.OrderBy(p => p.Title);                                // 2
var titles = query.Select(p => p.Title);                            // 3
List<string> list = titles.ToList();                                // 4
```

- A) 1
- B) 2
- C) 3
- D) 4

<details><summary>Ответ</summary>

**D.** `Where`, `OrderBy` и `Select` над `IQueryable` только строят описание запроса. SQL уходит при перечислении: `ToList`, `First`, `Count`, `foreach`. Все три оператора попадут в один SQL-запрос.
→ [Базовая теория](basics.ru.md#запросы-linq-превращается-в-sql)

</details>

## 2. 🟢 Что выведет код?

```csharp
Blog blog = context.Blogs.Single(b => b.Id == 1);
Console.WriteLine(blog.Posts.Count);
```

- A) `2`: EF загрузит посты при обращении к навигации
- B) `0`
- C) `NullReferenceException`
- D) `InvalidOperationException`: навигация не загружена

<details><summary>Ответ</summary>

**B.** Без `Include` и без ленивой загрузки навигация не загружается, а список инициализирован пустым (`= []`). Ошибки нет, поэтому баг легко пропустить. Нужен `Include(b => b.Posts)` или проекция. Вариант A верен только при включённой ленивой загрузке.
→ [Базовая теория](basics.ru.md#связанные-данные-include)

</details>

## 3. 🟢 Блог загружен обычным запросом, затем изменено только его свойство `Name`. Какой `UPDATE` сгенерирует `SaveChanges`?

- A) обновит все столбцы таблицы `Blogs`
- B) обновит только изменённые столбцы
- C) никакой: без вызова `Update(blog)` изменения не сохраняются
- D) удалит строку и вставит новую

<details><summary>Ответ</summary>

**B.** Отслеживаемая сущность хранит снимок исходных значений, и EF записывает только изменённые свойства: `UPDATE "Blogs" SET "Name" = @p0 …`. Вызывать `Update` не нужно — наоборот, `Update` пометил бы изменёнными все свойства, и тогда получился бы вариант A.
→ [Базовая теория](basics.ru.md#сохранение-savechanges)

</details>

## 4. 🟢 Что вернёт `SaveChanges()`?

```csharp
Blog blog = context.Blogs.AsNoTracking().Single(b => b.Id == 1);
blog.Name = "Changed";
int saved = context.SaveChanges();
```

- A) `1`
- B) `0`
- C) исключение: сущность не отслеживается
- D) `2`

<details><summary>Ответ</summary>

**B.** Сущность из `AsNoTracking` находится в состоянии `Detached`: контекст о ней не знает и изменения не видит. Ошибки нет — сохранять просто нечего.
→ [Базовая теория](basics.ru.md#отслеживание-изменений)

</details>

## 5. 🟢 Блог с `Id = 1` уже загружен в этот контекст. Сколько SQL-команд выполнит `context.Blogs.Find(1)`?

- A) 0
- B) 1
- C) 2: проверка существования и загрузка
- D) зависит от размера таблицы

<details><summary>Ответ</summary>

**A.** `Find` сначала ищет сущность среди отслеживаемых и идёт в базу, только если её там нет. `Single(b => b.Id == 1)` выполнил бы SQL в любом случае.
→ [Базовая теория](basics.ru.md#отслеживание-изменений)

</details>

## 6. 🟢 Какие утверждения о миграциях верны? (несколько ответов)

- A) `dotnet ef migrations add` создаёт класс миграции с методами `Up` и `Down` и обновляет снимок модели
- B) `dotnet ef migrations add` сразу меняет схему базы
- C) `dotnet ef database update` применяет миграции и записывает их в `__EFMigrationsHistory`
- D) базу, созданную `EnsureCreated()`, можно потом обновлять миграциями

<details><summary>Ответ</summary>

**A, C.** `migrations add` только генерирует код, базу меняет `database update` (или `Migrate()`, скрипт, бандл). `EnsureCreated` не создаёт таблицу истории миграций, и последующий `Migrate()` падает на уже существующих таблицах.
→ [Базовая теория](basics.ru.md#миграции)

</details>

## 7. 🟡 Ленивая загрузка включена. Сколько SQL-команд выполнит код?

```csharp
int total = 0;
foreach (Blog blog in context.Blogs.ToList())
{
    total += blog.Posts.Count;
}
```

- A) 1
- B) 2
- C) 4
- D) 7

<details><summary>Ответ</summary>

**C.** Один запрос за блогами и по запросу на посты каждого из 3 блогов — проблема N+1. Для 1000 блогов было бы 1001 команд. Проекция `Select(b => new { b.Name, Posts = b.Posts.Count })` делает то же за 1 команду.
→ [Продвинутая теория](advanced.ru.md#загрузка-связанных-данных-и-проблема-n1)

</details>

## 8. 🟡 `Shorten` — обычный C#-метод. Что произойдёт?

```csharp
var a = context.Posts.Select(p => Shorten(p.Title)).ToList();
var b = context.Posts.Where(p => Shorten(p.Title).EndsWith("…")).ToList();
```

- A) оба запроса выполнятся в базе
- B) оба бросят исключение
- C) `a` выполнится, `b` бросит `InvalidOperationException`
- D) оба выполнятся: EF молча загрузит таблицу и отфильтрует в памяти

<details><summary>Ответ</summary>

**C.** Клиентская оценка разрешена только в последнем `Select`: EF загружает заголовки и вызывает `Shorten` в памяти. В `Where` метод должен выполниться в базе, а перевести его в SQL нельзя — исключение «could not be translated». Вариант D описывает поведение EF Core 2.x, от которого отказались, чтобы не выгружать таблицы незаметно.
→ [Продвинутая теория](advanced.ru.md#что-ef-умеет-перевести-в-sql)

</details>

## 9. 🟡 Что выведет код? У поста 1 было 10 лайков.

```csharp
Post post = context.Posts.Single(p => p.Id == 1);
context.Posts.Where(p => p.Id == 1)
    .ExecuteUpdate(s => s.SetProperty(p => p.Likes, 1000));
Console.WriteLine(post.Likes);
```

- A) `1000`
- B) `10`
- C) `0`
- D) исключение: сущность отслеживается

<details><summary>Ответ</summary>

**B.** `ExecuteUpdate` сразу выполняет SQL и не трогает change tracker. В базе `1000`, а загруженный объект устарел. Чтобы увидеть новое значение, нужен `context.Entry(post).Reload()` или новый контекст.
→ [Продвинутая теория](advanced.ru.md#executeupdate-и-executedelete)

</details>

## 10. 🟡 Найди ошибку

```csharp
public int CountByName(string name) =>
    context.Blogs.FromSqlRaw("SELECT * FROM Blogs WHERE Name = '" + name + "'").Count();
```

<details><summary>Ответ</summary>

SQL-инъекция: значение вклеено в текст запроса. Ввод `x' OR '1'='1` возвращает все 3 блога вместо 0. Анализатор EF выдаёт на этот код предупреждение EF1003. Исправление: `FromSql($"SELECT * FROM Blogs WHERE Name = {name}")` — значение станет параметром `@p0`. Ещё проще — обычный LINQ: `context.Blogs.Count(b => b.Name == name)`.
→ [Продвинутая теория](advanced.ru.md#сырой-sql)

</details>

## 11. 🟡 Свойство `Version` настроено как `IsConcurrencyToken()`. Два пользователя загрузили один блог, оба изменили имя и увеличили `Version`. Первый сохранил. Что произойдёт при сохранении второго?

- A) изменения второго пользователя перезапишут изменения первого
- B) `UPDATE … WHERE "Id" = @id AND "Version" = @old` затронет 0 строк, EF бросит `DbUpdateConcurrencyException`
- C) EF заблокирует строку, и второй пользователь будет ждать
- D) EF автоматически объединит изменения обоих

<details><summary>Ответ</summary>

**B.** Это оптимистическая конкурентность: исходное значение токена попадает в `WHERE`, и если строку уже изменили, `UPDATE` ничего не находит. Решать конфликт должно приложение: перечитать данные (`Reload`) или повторить сохранение со свежими исходными значениями. Блокировок (C) EF не ставит, и сам ничего не объединяет (D). A — то, что произошло бы без токена или если забыть увеличить `Version`.
→ [Продвинутая теория](advanced.ru.md#оптимистическая-конкурентность)

</details>

## 12. 🟡 Что не так с кодом и как его исправить?

```csharp
await Parallel.ForEachAsync(blogIds, async (id, ct) =>
{
    Blog blog = await _context.Blogs.SingleAsync(b => b.Id == id, ct);
    blog.Name = blog.Name.Trim();
    await _context.SaveChangesAsync(ct);
});
```

<details><summary>Ответ</summary>

Один `DbContext` используется из нескольких параллельных задач, а он не потокобезопасен. Когда операции пересекаются, EF бросает `InvalidOperationException`: «A second operation was started on this context instance before a previous operation completed», а если контекст ещё инициализируется — «An attempt was made to use the context instance while it is being configured». В 50 прогонах на SQLite упал каждый, причём с обоими сообщениями. Но в общем случае ошибка зависит от того, пересеклись ли операции по времени, поэтому прогон без ошибки ничего не доказывает. Исправления: создавать отдельный контекст на каждую задачу через `IDbContextFactory<T>`; или обрабатывать последовательно; или одним запросом загрузить все блоги, изменить и сделать один `SaveChanges`.
→ [Продвинутая теория](advanced.ru.md#жизненный-цикл-dbcontext)

</details>

## 13. 🔴 Иерархия `Payment` → `CardPayment`, `CashPayment`. Какая стратегия не создаёт таблицу для базового типа, а запрос к `Payments` выполняет через `UNION ALL`?

- A) TPH
- B) TPT
- C) TPC
- D) ни одна: EF всегда создаёт таблицу для базового типа

<details><summary>Ответ</summary>

**C.** TPC (table per concrete type) создаёт полную таблицу для каждого неабстрактного типа, и полиморфный запрос объединяет их. TPH — одна таблица с дискриминатором; TPT — таблица базового типа плюс таблицы подтипов, соединяемые `JOIN`. Ключи в TPC должны быть уникальны во всей иерархии, поэтому в примере используется `Guid`.
→ [Экспертная теория](expert.ru.md#наследование-tph-tpt-tpc)

</details>

## 14. 🔴 Что выведет код?

```csharp
string name = "x' OR '1'='1";
Console.WriteLine(context.Blogs.FromSqlRaw($"SELECT * FROM Blogs WHERE Name = '{name}'").Count());
```

- A) `0`: интерполированная строка превращается в параметр
- B) `3`
- C) исключение синтаксиса SQL
- D) не скомпилируется

<details><summary>Ответ</summary>

**B.** `FromSqlRaw` принимает `string`, поэтому C# подставляет значение в текст ещё до вызова EF — это инъекция, и условие `OR '1'='1'` возвращает все блоги. Параметром интерполяция становится только в `FromSql`, который принимает `FormattableString`. Код компилируется, но анализатор выдаёт предупреждение EF1002.
→ [Экспертная теория](expert.ru.md#fromsqlraw-с-интерполяцией)

</details>

## 15. 🔴 Что вернут два вызова `SaveChanges()`?

```csharp
context.ChangeTracker.AutoDetectChangesEnabled = false;
Blog blog = context.Blogs.Single(b => b.Id == 3);
blog.Name = "Changed";
int first = context.SaveChanges();
context.ChangeTracker.DetectChanges();
int second = context.SaveChanges();
```

- A) `1` и `0`
- B) `0` и `1`
- C) `0` и `0`
- D) `1` и `1`

<details><summary>Ответ</summary>

**B.** EF находит изменения, сравнивая текущие значения со снимком в `DetectChanges()`. Обычно `SaveChanges` вызывает его сам, но здесь автообнаружение выключено: первый вызов не видит изменений и возвращает `0`. После явного `DetectChanges()` сущность становится `Modified`, и второй вызов сохраняет `1` строку.
→ [Экспертная теория](expert.ru.md#как-отслеживаются-изменения)

</details>
