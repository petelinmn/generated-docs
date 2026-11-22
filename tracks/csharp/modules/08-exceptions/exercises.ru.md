---
title: "Упражнения: Исключения"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 08-exceptions
part: 1
tags: [exceptions, custom-exception, rethrow, exception-filters, retry, finally]
status: draft
updated: 2026-11-22
---

# Упражнения: Исключения

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Собственное исключение

**Задача:** напиши `InsufficientFundsException` со свойством `Shortfall` (сколько не хватает), сообщением `Short by N.` и вторым конструктором, принимающим внутреннее исключение. Используй его в методе снятия со счёта.

**Ожидаемый результат:**

```text
баланс 100, снять 130 → "Short by 30." | shortfall = 30
конструктор с внутренним исключением → InnerException.Message = "inner"
```

<details><summary>Подсказка</summary>

Наследуй от `Exception`; сообщение передавай в `base(...)`; свойство только для чтения.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: a custom exception with data and an inner exception.
class InsufficientFundsException : Exception
{
    public decimal Shortfall { get; }

    public InsufficientFundsException(decimal shortfall)
        : base($"Short by {shortfall}.") => Shortfall = shortfall;

    public InsufficientFundsException(decimal shortfall, Exception inner)
        : base($"Short by {shortfall}.", inner) => Shortfall = shortfall;
}

class Acct(decimal b)
{
    public void Withdraw(decimal a)
    {
        if (a > b) throw new InsufficientFundsException(a - b);
        b -= a;
    }
}
```

Проверено: `Short by 30. | shortfall=30` и `inner`. Данные об ошибке лежат в свойствах — вызывающему не нужно разбирать текст сообщения.

</details>

## 2. 🟢 Порядок try/catch/finally

**Задача:** напиши `Divide(int a, int b, List<string> log)`, который записывает в `log` этапы `try`, `catch`, `finally` и возвращает результат деления или `"error"` при делении на ноль.

**Ожидаемый результат:**

```text
Divide(10, 2) → "5",     лог: try>finally
Divide(1, 0)  → "error", лог: try>catch>finally
```

<details><summary>Подсказка</summary>

`finally` должен записываться и при нормальном завершении, и при исключении.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: finally runs on both the normal and the exceptional path.
static string Divide(int a, int b, List<string> log)
{
    try { log.Add("try"); return (a / b).ToString(); }
    catch (DivideByZeroException) { log.Add("catch"); return "error"; }
    finally { log.Add("finally"); }
}
```

Проверено: `5 try>finally` и `error try>catch>finally`.

</details>

## 3. 🟡 Перебросить, не потеряв стек

**Задача:** метод `Outer` вызывает `Thrower`, логирует ошибку и перебрасывает её. Покажи, что `throw;` сохраняет в стеке кадр `Thrower`, а `throw ex;` — нет.

**Ожидаемый результат:**

```text
Outer (throw;)    → стек содержит Thrower: True
OuterBad (throw ex;) → стек содержит Thrower: False
```

<details><summary>Подсказка</summary>

Проверяй `e.StackTrace.Contains("Thrower")` в вызывающем коде.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: rethrow with throw; to keep the original stack trace.
static void Thrower() => throw new InvalidOperationException("x");
static void Outer() { try { Thrower(); } catch (Exception) { Console.WriteLine("logged"); throw; } }
static void OuterBad() { try { Thrower(); } catch (Exception ex) { throw ex; } }

try { Outer(); } catch (Exception e) { Console.WriteLine(e.StackTrace!.Contains("Thrower")); }     // True
try { OuterBad(); } catch (Exception e) { Console.WriteLine(e.StackTrace!.Contains("Thrower")); }  // False
```

Проверено: `True` и `False`. Если нужно только залогировать — используй фильтр `when`, он не трогает стек вообще.

</details>

## 4. 🟡 Повтор только для временных ошибок

**Задача:** напиши `Retry<T>(Func<T> op, int max)`, который повторяет операцию, **только** если выброшен `TimeoutException`, и не больше `max` попыток; любое другое исключение должно выйти сразу.

**Ожидаемый результат:**

```text
операция падает с TimeoutException дважды, на 3-й успех → 3
операция бросает ArgumentException                      → ArgumentException сразу ("bad")
```

<details><summary>Подсказка</summary>

`catch (Exception e) when (e is TimeoutException && i < max) { }` — фильтр решает, ловить ли исключение; последняя попытка уже не ловится.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: exception filters decide which errors are retried.
static T Retry<T>(Func<T> op, int max)
{
    for (int i = 1; ; i++)
    {
        try { return op(); }
        catch (Exception e) when (e is TimeoutException && i < max) { }
    }
}
```

Проверено: на трёх попытках результат `3`, `ArgumentException` не повторялся. В реальных проектах такую логику даёт Polly (библиотека ещё не создана).

</details>

## 5. 🔴 Исключение в finally

**Задача:** в методе `Bad` в `try` бросается `original`, а `finally` выполняет очистку, которая бросает `cleanup failed`. Покажи, что вызывающий видит только `cleanup failed`, и исправь так, чтобы вызывающий получал `original`.

**Ожидаемый результат:**

```text
Bad  → "cleanup failed" (исходная ошибка потеряна)
Good → "original"
```

<details><summary>Подсказка</summary>

Очистку в `finally` оберни в собственный `try/catch`, ошибку очистки залогируй, но не пробрасывай.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: cleanup errors must not mask the original exception.
static void Bad()
{
    try { throw new Exception("original"); }
    finally { throw new Exception("cleanup failed"); }
}

static void Good()
{
    try { throw new Exception("original"); }
    finally
    {
        try { throw new Exception("cleanup failed"); }
        catch (Exception) { /* log it, never mask the original */ }
    }
}
```

Проверено: `bad: cleanup failed` и `good: original`.

</details>
