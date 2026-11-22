---
title: "Quiz: Exceptions"
lang: en
track: csharp
kind: module
doc: quiz
slug: 08-exceptions
part: 1
tags: [exceptions, try-catch-finally, exception-filters, rethrow, aggregate-exception]
status: draft
updated: 2026-11-22
---

# Quiz: Exceptions

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 In what order do the blocks run for `Divide(1, 0)`?

```csharp
static string Divide(int a, int b, List<string> log)
{
    try { log.Add("try"); return (a / b).ToString(); }
    catch (DivideByZeroException) { log.Add("catch"); return "error"; }
    finally { log.Add("finally"); }
}
```

- A) `try > finally`
- B) `try > catch > finally`
- C) `catch > try > finally`
- D) `try > finally > catch`

<details><summary>Answer</summary>

**B.** First `try` up to the exception, then the matching `catch`, and `finally` always runs — both on normal completion (`try > finally`) and on an exception (verified).
→ [Basics](basics.en.md#try-catch-finally)

</details>

## 2. 🟢 How do you correctly rethrow a caught exception while keeping its stack?

- A) `throw ex;`
- B) `throw;`
- C) `throw new Exception(ex.Message);`
- D) `return ex;`

<details><summary>Answer</summary>

**B.** A bare `throw;` preserves the original trace. `throw ex;` restarts the trace: the frame of the method where the error occurred will not be in it (verified). `new Exception(ex.Message)` loses the type and the stack unless you pass `ex` as the inner exception.
→ [Advanced theory](advanced.en.md#throw-and-throw-ex)

</details>

## 3. 🟢 Select all correct statements

- [ ] A) `finally` also runs on `return` inside `try`
- [ ] B) a custom exception derives from `Exception`
- [ ] C) exceptions suit an ordinary expected outcome, such as "user not found"
- [ ] D) a more specific `catch` must come above a general one

<details><summary>Answer</summary>

**A, B, D.** C is wrong: throwing is expensive — 20,000 throws ≈ 37 ms versus ≈ 0.3 ms for `TryParse` (verified); for expected outcomes use `TryXxx` or a return value. D: a general `catch` before a specific one is a compile error.
→ [Basics](basics.en.md#when-to-throw-an-exception)

</details>

## 4. 🟢 What is better than an exception when parsing a user string into a number?

- A) `int.Parse` in `try/catch`
- B) `int.TryParse`
- C) `Convert.ToInt32` in `try/catch`
- D) it makes no difference

<details><summary>Answer</summary>

**B.** Invalid input is a routine situation, so `TryParse` returns `false` without an exception and is dozens of times cheaper.
→ [Basics](basics.en.md#when-to-throw-an-exception)

</details>

## 5. 🟡 What does the method return?

```csharp
static int F()
{
    int x = 1;
    try { return x; }
    finally { x = 99; }
}
```

- A) `99`
- B) `1`
- C) an exception
- D) a compile error

<details><summary>Answer</summary>

**B.** The return value is computed and fixed before `finally` runs; changing the local `x` does not affect the result (verified). But if a **reference** to a mutable object is returned, `finally` will change it.
→ [Advanced theory](advanced.en.md#finally-details)

</details>

## 6. 🟡 What does the calling code receive?

```csharp
static void M() { try { throw new Exception("original"); } finally { throw new Exception("cleanup failed"); } }
```

- A) `original`
- B) `cleanup failed`, the original error is lost
- C) an `AggregateException` with both
- D) `cleanup failed` with `InnerException = original`

<details><summary>Answer</summary>

**B.** An exception from `finally` replaces the original one without an `InnerException` (verified). So cleanup code in `finally` should be wrapped in its own `try/catch` (log it, but do not mask the original).
→ [Expert theory](expert.en.md#an-exception-in-finally-replaces-the-original)

</details>

## 7. 🟡 What happens with a filter that returns `false`?

```csharp
try { throw new InvalidOperationException(); }
catch (Exception e) when (Log(e)) { Console.WriteLine("caught"); }   // Log prints and returns false
```

- A) `caught`
- B) the exception is not caught, but `Log` ran (handy for logging)
- C) the exception is silently swallowed
- D) a compile error

<details><summary>Answer</summary>

**B.** A `when` filter runs before the stack is unwound; on `false` the exception moves on to other handlers. This is a handy way to log, but the filter itself must not throw. The order is `filter → inner finally → catch` (verified).
→ [Advanced theory](advanced.en.md#exception-filters-when)

</details>

## 8. 🟡 What exception type does the caller get?

```csharp
var t = Task.Run(() => throw new InvalidOperationException("x"));
try { t.Wait(); } catch (Exception e) { Console.WriteLine(e.GetType().Name); }
try { await t; }   catch (Exception e) { Console.WriteLine(e.GetType().Name); }
```

<details><summary>Answer</summary>

```text
AggregateException
InvalidOperationException
```

`Wait()`/`Result` wrap exceptions in an `AggregateException`, while `await` unwraps and throws the original (verified). More in [10 · Async/await](../10-async-await/overview.en.md).
→ [Expert theory](expert.en.md#wait-wraps-await-unwraps)

</details>

## 9. 🟡 Open question: when do you throw an exception and when do you return a result or `false`?

<details><summary>Key points of a good answer</summary>

- an exception is for exceptional, unforeseen situations: a broken method contract, a corrupted state, an unavailable resource;
- expected failures (user input, "not found") — `TryXxx`, `null`, `Result` objects;
- throwing is expensive and disrupts control flow; argument checks — `ArgumentNullException.ThrowIfNull` and the like;
- do not swallow exceptions silently, catch the narrowest possible type, add context via `InnerException`.

</details>

## 10. 🔴 What does the code print and why?

```csharp
static void Thrower() => throw new InvalidOperationException("x");
static void OuterBad() { try { Thrower(); } catch (Exception ex) { throw ex; } }

try { OuterBad(); } catch (Exception e) { Console.WriteLine(e.StackTrace!.Contains("Thrower")); }
```

<details><summary>Answer</summary>

`False`. `throw ex;` compiles to the IL instruction `throw`, which resets the trace and starts it at the rethrow site, so the `Thrower` frame is lost. `throw;` compiles to `rethrow` and keeps it (in the verified variant with `throw;` the result is `True`).
→ [Expert theory](expert.en.md#throw-ex-loses-the-stack)

</details>

## 11. 🔴 What do these expressions give?

```csharp
Console.WriteLine(1.0 / 0);
Console.WriteLine(1 / 0);
```

<details><summary>Answer</summary>

The first prints `∞` (depending on the culture — `Infinity`) with no exception: floating-point division follows IEEE 754. The second throws `DivideByZeroException`: integer division by zero is an error (verified).
→ [Expert theory](expert.en.md#division-integer-and-floating-point)

</details>
