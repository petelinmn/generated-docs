---
title: "Упражнения: Dapper"
lang: ru
track: csharp
kind: library
doc: exercises
slug: dapper
part: 2
tags: [dapper, sql, multi-mapping, transactions]
status: draft
updated: 2026-11-22
---

# Упражнения: Dapper

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском (Dapper 2.1.100 + SQLite in-memory). Запускаемых версий в `code/` нет (уровня 5 нет). Таблица: `Users(Id, Name, Email)` с `Ann`, `Bob`, `Cid`.

## 1. 🟢 Вставка и выборка

**Задача:** вставьте трёх пользователей одним вызовом `Execute` и выберите тех, у кого `Id > 1`.

**Ожидаемый результат:** `rows inserted: 3`; `Bob`, `Cid`.

<details><summary>Подсказка</summary>

`Execute(sql, IEnumerable<object>)` и `Query<User>(sql, new { min = 1 })`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: batch insert and parameterized select.
var n = cn.Execute("INSERT INTO Users(Name, Email) VALUES (@Name, @Email)",
    new[] { new { Name = "Ann", Email = "a@x" }, new { Name = "Bob", Email = "b@x" }, new { Name = "Cid", Email = (string?)null } });
var users = cn.Query<User>("SELECT Id, Name, Email FROM Users WHERE Id > @min ORDER BY Id", new { min = 1 }).ToList();
```

Проверено: `rows inserted: 3`, `2:Bob:b@x,3:Cid:null`.

</details>

## 2. 🟡 Инъекция

**Задача:** покажите разницу между параметром и конкатенацией на вводе `x' OR '1'='1`.

**Ожидаемый результат:** 0 строк с параметром; 3 строки с конкатенацией.

<details><summary>Подсказка</summary>

`Query<User>("… WHERE Name = @n", new { n = evil })` против строки `"… = '" + evil + "'"`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: parameters prevent SQL injection.
var evil = "x' OR '1'='1";
var safe = cn.Query<User>("SELECT * FROM Users WHERE Name = @n", new { n = evil }).Count();             // 0
var unsafeCount = cn.Query<User>("SELECT * FROM Users WHERE Name = '" + evil + "'").Count();            // 3
```

Проверено: `param injection result: 0`, `concat injection result: 3`.

</details>

## 3. 🟡 JOIN в объекты

**Задача:** загрузите заказы с вложенным пользователем.

**Ожидаемый результат:** `1:Ann,2:Ann,3:Bob`.

<details><summary>Подсказка</summary>

`Query<Order, User, Order>(sql, (o, u) => { o.User = u; return o; }, splitOn: "Id")`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: multi-mapping.
var joined = cn.Query<Order, User, Order>(
    "SELECT o.Id, o.Total, u.Id, u.Name FROM Orders o JOIN Users u ON u.Id = o.UserId",
    (o, u) => { o.User = u; return o; }, splitOn: "Id");
```

Проверено: `multi-map: 1:Ann,2:Ann,3:Bob`.

</details>

## 4. 🔴 Сопоставление имён и транзакция

**Задача:** (а) заставьте столбец `created_at` попасть в `CreatedAt` для **нового** типа; (б) выполните вставку в транзакции и откатите её.

**Ожидаемый результат:** `CreatedAt=2026-01-01` для нового типа (для старого остаётся `null`); число строк после отката не изменилось (3).

<details><summary>Подсказка</summary>

`DefaultTypeMap.MatchNamesWithUnderscores = true` действует на ещё не сопоставленные типы; `transaction: tx`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: name mapping (set before first use of a type) and rollback.
DefaultTypeMap.MatchNamesWithUnderscores = true;
var created = cn.QueryFirst<UserCreated2>("SELECT Id, created_at FROM Users");   // new type -> mapped

using var tx = cn.BeginTransaction();
cn.Execute("INSERT INTO Users(Name) VALUES ('Tmp')", transaction: tx);
tx.Rollback();
```

Проверено: новый тип — `2026-01-01`, уже использованный — `null`; после отката `count = 3`.

</details>
