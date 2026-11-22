---
title: "Exercises: Dapper"
lang: en
track: csharp
kind: library
doc: exercises
slug: dapper
part: 2
tags: [dapper, sql, multi-mapping, transactions]
status: draft
updated: 2026-11-22
---

# Exercises: Dapper

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running (Dapper 2.1.100 + SQLite in-memory). There are no runnable versions in `code/` (level 5 does not exist). The table: `Users(Id, Name, Email)` with `Ann`, `Bob`, `Cid`.

## 1. 🟢 Insert and select

**Task:** insert three users with one `Execute` call and select those with `Id > 1`.

**Expected result:** `rows inserted: 3`; `Bob`, `Cid`.

<details><summary>Hint</summary>

`Execute(sql, IEnumerable<object>)` and `Query<User>(sql, new { min = 1 })`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: batch insert and parameterized select.
var n = cn.Execute("INSERT INTO Users(Name, Email) VALUES (@Name, @Email)",
    new[] { new { Name = "Ann", Email = "a@x" }, new { Name = "Bob", Email = "b@x" }, new { Name = "Cid", Email = (string?)null } });
var users = cn.Query<User>("SELECT Id, Name, Email FROM Users WHERE Id > @min ORDER BY Id", new { min = 1 }).ToList();
```

Verified: `rows inserted: 3`, `2:Bob:b@x,3:Cid:null`.

</details>

## 2. 🟡 Injection

**Task:** show the difference between a parameter and concatenation on the input `x' OR '1'='1`.

**Expected result:** 0 rows with a parameter; 3 rows with concatenation.

<details><summary>Hint</summary>

`Query<User>("… WHERE Name = @n", new { n = evil })` against the string `"… = '" + evil + "'"`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: parameters prevent SQL injection.
var evil = "x' OR '1'='1";
var safe = cn.Query<User>("SELECT * FROM Users WHERE Name = @n", new { n = evil }).Count();             // 0
var unsafeCount = cn.Query<User>("SELECT * FROM Users WHERE Name = '" + evil + "'").Count();            // 3
```

Verified: `param injection result: 0`, `concat injection result: 3`.

</details>

## 3. 🟡 A JOIN into objects

**Task:** load orders with a nested user.

**Expected result:** `1:Ann,2:Ann,3:Bob`.

<details><summary>Hint</summary>

`Query<Order, User, Order>(sql, (o, u) => { o.User = u; return o; }, splitOn: "Id")`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: multi-mapping.
var joined = cn.Query<Order, User, Order>(
    "SELECT o.Id, o.Total, u.Id, u.Name FROM Orders o JOIN Users u ON u.Id = o.UserId",
    (o, u) => { o.User = u; return o; }, splitOn: "Id");
```

Verified: `multi-map: 1:Ann,2:Ann,3:Bob`.

</details>

## 4. 🔴 Name mapping and a transaction

**Task:** (a) make the column `created_at` land in `CreatedAt` for a **new** type; (b) run an insert in a transaction and roll it back.

**Expected result:** `CreatedAt=2026-01-01` for the new type (it stays `null` for the old one); the row count did not change after the rollback (3).

<details><summary>Hint</summary>

`DefaultTypeMap.MatchNamesWithUnderscores = true` applies to types not yet mapped; `transaction: tx`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: name mapping (set before first use of a type) and rollback.
DefaultTypeMap.MatchNamesWithUnderscores = true;
var created = cn.QueryFirst<UserCreated2>("SELECT Id, created_at FROM Users");   // new type -> mapped

using var tx = cn.BeginTransaction();
cn.Execute("INSERT INTO Users(Name) VALUES ('Tmp')", transaction: tx);
tx.Rollback();
```

Verified: the new type — `2026-01-01`, the already used one — `null`; after the rollback `count = 3`.

</details>
