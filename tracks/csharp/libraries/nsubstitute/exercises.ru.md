---
title: "Упражнения: NSubstitute"
lang: ru
track: csharp
kind: library
doc: exercises
slug: nsubstitute
part: 2
tags: [nsubstitute, mocking, callbacks, partial-substitutes]
status: draft
updated: 2026-11-22
---

# Упражнения: NSubstitute

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском (NSubstitute 6.2.0, консольное приложение, без тестового фреймворка). Запускаемых версий в `code/` нет (уровня 5 нет). Типы в примерах — `public` (иначе прокси не создаётся).

## 1. 🟢 Настройка и проверка

**Задача:** подмените `ICalculator`: `Add(1, 2)` → `3`, `Add(any, 10)` → `100`. Проверьте, что `Add(9, 9)` не вызывался, а `Add(1, 2)` — вызывался.

**Ожидаемый результат:** `3`, `100`, ненастроенный `Add(7, 7)` → `0`; проверки проходят.

<details><summary>Подсказка</summary>

`Substitute.For<ICalculator>()`, `.Returns(...)`, `Received()`, `DidNotReceive()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: configure, then verify interactions.
var calc = Substitute.For<ICalculator>();
calc.Add(1, 2).Returns(3);
calc.Add(Arg.Any<int>(), 10).Returns(100);
Console.WriteLine($"{calc.Add(1, 2)} {calc.Add(5, 10)} {calc.Add(7, 7)}");   // 3 100 0
calc.Received().Add(1, 2);
calc.DidNotReceive().Add(9, 9);
```

Проверено: `configured: 3 100 unconfigured=0`.

</details>

## 2. 🟡 Результат из аргументов и последовательность

**Задача:** (а) `Add` возвращает сумму аргументов; (б) подмена, возвращающая `1, 2, 3`, при четырёх вызовах.

**Ожидаемый результат:** `42` для `Add(20, 22)`; `1233`.

<details><summary>Подсказка</summary>

`Returns(c => c.ArgAt<int>(0) + c.ArgAt<int>(1))` и `Returns(1, 2, 3)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: computed result and a result sequence.
echo.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(c => c.ArgAt<int>(0) + c.ArgAt<int>(1));
seq.Add(0, 0).Returns(1, 2, 3);
```

Проверено: `returns from args: 42`, `sequence: 1233`.

</details>

## 3. 🟡 Async и ошибки

**Задача:** `IRepo.GetAsync(int)`: для `2` вернуть `"two"`, для `99` — `KeyNotFoundException` при `await`; ненастроенный вызов — `null`.

**Ожидаемый результат:** `two`; `KeyNotFoundException`; `null`.

<details><summary>Подсказка</summary>

`Returns("two")` для `Task<string?>`, `ThrowsAsync(new KeyNotFoundException())`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: Task<T> results and async failures.
repo.GetAsync(2).Returns("two");
repo.GetAsync(99).ThrowsAsync(new KeyNotFoundException());
var v = await repo.GetAsync(3);        // null
```

Проверено: `async: onetwo unconfigured=` (пусто — `null`), `ThrowsAsync: KeyNotFoundException`.

</details>

## 4. 🔴 Колбэк, событие и частичная подмена

**Задача:** (а) `When…Do` для записи аргументов `void`-метода; (б) вызвать событие подмены; (в) `ForPartsOf<Concrete>` с настроенным виртуальным методом и реальным невиртуальным.

**Ожидаемый результат:** `when:a,when:b`; обработчик вызван 1 раз; `Virtual() = 8`, `NonVirtual() = 2`; `Returns` на невиртуальном — `CouldNotSetReturnDueToNoLastCallException`.

<details><summary>Подсказка</summary>

`sub.When(l => l.Log(Arg.Any<string>())).Do(c => …)`, `pub.Changed += Raise.Event()`, `Substitute.ForPartsOf<Concrete>()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: callbacks, events and partial substitutes.
logger.When(l => l.Log(Arg.Any<string>())).Do(c => cap.Add("when:" + c.Arg<string>()));
pub.Changed += (s, e) => got++;
pub.Changed += Raise.Event();

var part = Substitute.ForPartsOf<Concrete>();
part.Virtual().Returns(8);                 // Virtual() = 8; NonVirtual() runs the real code (2)
// Substitute.For<Concrete>().NonVirtual().Returns(5) -> CouldNotSetReturnDueToNoLastCallException
```

Проверено: `captured: when:a,when:b,…`, `event raised handlers: 1`, `ForPartsOf virtual: 8 nonvirtual real: 2`, `non-virtual Returns: CouldNotSetReturnDueToNoLastCallException`.

</details>
