---
title: "Exercises: Exceptions"
lang: en
track: csharp
kind: module
doc: exercises
slug: 08-exceptions
part: 1
tags: [exceptions, custom-exception, rethrow, exception-filters, retry, finally]
status: draft
updated: 2026-11-22
---

# Exercises: Exceptions

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 A custom exception

**Task:** write `InsufficientFundsException` with a `Shortfall` property (how much is missing), the message `Short by N.` and a second constructor that accepts an inner exception. Use it in an account withdrawal method.

**Expected result:**

```text
balance 100, withdraw 130 → "Short by 30." | shortfall = 30
constructor with an inner exception → InnerException.Message = "inner"
```

<details><summary>Hint</summary>

Derive from `Exception`; pass the message to `base(...)`; make the property read-only.

</details>

<details><summary>Solution</summary>

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

Verified: `Short by 30. | shortfall=30` and `inner`. The error data lives in properties — the caller does not need to parse the message text.

</details>

## 2. 🟢 The order of try/catch/finally

**Task:** write `Divide(int a, int b, List<string> log)` that records the stages `try`, `catch`, `finally` in `log` and returns the division result or `"error"` on division by zero.

**Expected result:**

```text
Divide(10, 2) → "5",     log: try>finally
Divide(1, 0)  → "error", log: try>catch>finally
```

<details><summary>Hint</summary>

`finally` must be recorded both on normal completion and on an exception.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: finally runs on both the normal and the exceptional path.
static string Divide(int a, int b, List<string> log)
{
    try { log.Add("try"); return (a / b).ToString(); }
    catch (DivideByZeroException) { log.Add("catch"); return "error"; }
    finally { log.Add("finally"); }
}
```

Verified: `5 try>finally` and `error try>catch>finally`.

</details>

## 3. 🟡 Rethrow without losing the stack

**Task:** the method `Outer` calls `Thrower`, logs the error and rethrows it. Show that `throw;` keeps the `Thrower` frame in the stack, while `throw ex;` does not.

**Expected result:**

```text
Outer (throw;)       → the stack contains Thrower: True
OuterBad (throw ex;) → the stack contains Thrower: False
```

<details><summary>Hint</summary>

Check `e.StackTrace.Contains("Thrower")` in the calling code.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: rethrow with throw; to keep the original stack trace.
static void Thrower() => throw new InvalidOperationException("x");
static void Outer() { try { Thrower(); } catch (Exception) { Console.WriteLine("logged"); throw; } }
static void OuterBad() { try { Thrower(); } catch (Exception ex) { throw ex; } }

try { Outer(); } catch (Exception e) { Console.WriteLine(e.StackTrace!.Contains("Thrower")); }     // True
try { OuterBad(); } catch (Exception e) { Console.WriteLine(e.StackTrace!.Contains("Thrower")); }  // False
```

Verified: `True` and `False`. If you only need to log, use a `when` filter — it does not touch the stack at all.

</details>

## 4. 🟡 Retry only for transient errors

**Task:** write `Retry<T>(Func<T> op, int max)` that repeats the operation **only** if a `TimeoutException` is thrown, and at most `max` attempts; any other exception must propagate immediately.

**Expected result:**

```text
the operation fails with TimeoutException twice, succeeds on the 3rd → 3
the operation throws ArgumentException                               → ArgumentException immediately ("bad")
```

<details><summary>Hint</summary>

`catch (Exception e) when (e is TimeoutException && i < max) { }` — the filter decides whether to catch; the last attempt is no longer caught.

</details>

<details><summary>Solution</summary>

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

Verified: on three attempts the result is `3`, and `ArgumentException` was not retried. In real projects this logic comes from Polly (the library is not created yet).

</details>

## 5. 🔴 An exception in finally

**Task:** in the method `Bad` a `try` throws `original`, while `finally` runs cleanup that throws `cleanup failed`. Show that the caller sees only `cleanup failed`, and fix it so that the caller gets `original`.

**Expected result:**

```text
Bad  → "cleanup failed" (the original error is lost)
Good → "original"
```

<details><summary>Hint</summary>

Wrap the cleanup in `finally` in its own `try/catch`; log the cleanup error but do not propagate it.

</details>

<details><summary>Solution</summary>

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

Verified: `bad: cleanup failed` and `good: original`.

</details>
