---
title: "Exceptions: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 08-exceptions
part: 1
tags: [exception-filters, rethrow, exception-dispatch-info, aggregate-exception, finally, try-pattern, custom-exceptions, exception-cost, global-handlers]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Exceptions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Exception filters: when

A `when` condition specifies whether a `catch` block will handle the exception:

```csharp
try
{
    try { throw new InvalidOperationException("a"); }
    catch (InvalidOperationException e) when (e.Message == "b") { /* not for us */ }
}
catch (Exception) { Console.WriteLine("filter false → outer catch"); }
```

If the filter returned `false`, the exception keeps rising to the outer handlers, and the **stack is neither unwound nor lost**. The filter runs **before** the `finally` blocks of the inner `try`s: verified — for `try { try { throw } finally { inner } } catch when (filter) { catch }` the order is `filter`, `inner finally`, `catch`. So a filter can be used for logging without catching:

```csharp
catch (Exception e) when (LogAndReturnFalse(e)) { }   // logs, never catches
```

Verified: such a filter wrote a message and did not catch the exception. A side effect in a filter is a deliberate technique, but the filter must be fast and must not throw exceptions of its own (it would be swallowed).

## throw and throw ex

Inside a `catch` you can rethrow an exception in two ways, and they **differ**:

```csharp
try { Thrower(); } catch { throw; }              // keeps the original stack
try { Thrower(); } catch (Exception ex) { throw ex; }   // resets the stack to this line
```

Verified: the stack after `throw;` contains the `Thrower` frame (`Thrower > lambda > Where`), while after `throw ex;` only the lambda — that is, **the place of the real error is lost**. Use `throw;`. If you need to throw the exception on another thread or later, store it in an `ExceptionDispatchInfo`:

```csharp
ExceptionDispatchInfo? edi = null;
try { Thrower(); } catch (Exception e) { edi = ExceptionDispatchInfo.Capture(e); }
edi!.Throw();      // the original stack frames are preserved (Thrower is in the trace)
```

This is exactly how `await` propagates the exceptions of tasks.

## Wrapping and InnerException

```csharp
catch (Exception e) { throw new ApplicationException("wrapped", e); }
```

Wrap when you need to add context or move to a layer's level of abstraction (for example, a data access exception → a service exception). Always pass the original cause as the second argument. The tree of causes is visible through `InnerException`; `ToString()` prints the whole chain. Do not wrap without a reason — extra layers make the source harder to find.

## finally: details

- `finally` runs on `return`, `break`, `continue` and on an exception in `try`/`catch`.
- **The return value is fixed before `finally`**: for `return x;` with `finally { x = 99; }` the method returned `1`, and for a reference type `finally` can change the object's content (the list got a second element — verified).
- **An exception inside `finally` replaces the original**: `try { throw A } finally { throw B }` — the caller gets only `B`, and `InnerException` is `null` (verified). So there must be no code in `finally` that can throw.
- `return` in `finally` is forbidden (CS0157): you cannot leave a `finally`.
- `finally` does not run if the process is terminated abnormally (`Environment.FailFast`, a power loss, `StackOverflow`).

## AggregateException

Operations over several tasks can finish with several errors, so they are collected by an `AggregateException` (more in 10 · Async/await):

```csharp
try { Task.WaitAll(Task.Run(() => throw new InvalidOperationException("t1")),
                   Task.Run(() => throw new ArgumentException("t2"))); }
catch (AggregateException ae)
{
    Console.WriteLine(string.Join(",", ae.InnerExceptions.Select(x => x.GetType().Name)));
}
```

Verified: `WaitAll` collected both exceptions; `Task.Wait()` for a single failed task also wraps the exception in an `AggregateException`, while `await` **unwraps** it and throws the original (`InvalidOperationException`). The `Flatten()` and `Handle(...)` methods help take apart nested aggregates.

## Designing your own exceptions

- Inherit from `Exception` or a suitable descendant (`InvalidOperationException`, `ArgumentException`), not from `ApplicationException`.
- The name ends with `Exception`.
- Pass the message and `InnerException` to the base constructor; support the needed constructors (`()`, `(string)`, `(string, Exception)`) if the exception is public and extensible.
- Add properties with data (`OrderId`) so that the calling code does not parse the message string.
- The message is for developers and logs; do not show it to the user: it may contain internal details.
- Create your own type only if it makes sense to **catch it separately**.

The dictionary `ex.Data["k"] = value` lets you attach context to an existing exception without creating a new type.

## Exceptions and performance

Exceptions are meant for exceptional cases. Throwing and catching is orders of magnitude more expensive than an ordinary return: the stack is collected, and the exception is unwound through the frames. In the check, 20,000 `throw`/`catch` cycles took ≈37 ms, while 20,000 `int.TryParse` calls with a failed parse took ≈0.3 ms (a difference of more than 100 times; the absolute numbers depend on the machine).

**The Try pattern**: instead of an exception the method returns a `bool` and the value through `out`:

```csharp
public static bool TryParseId(string text, out int id) => int.TryParse(text, out id) && id > 0;
```

The rule: if a failure is an **expected** part of the work (user input, a lookup by key), use `TryXxx`; if a failure means a contract violation — an exception.

## Global handlers

An unhandled exception terminates the process. To log before termination there is the event `AppDomain.CurrentDomain.UnhandledException`; in web applications exceptions are caught by middleware (14 · ASP.NET Core: Minimal API). A global handler does not replace local handling: it exists to **record** a failure, not to "fix" it.

## Best practices

- Catch specific exceptions where you know what to do; let the rest rise higher.
- `throw;`, not `throw ex;`.
- Do not swallow exceptions; if you catch in order to log, rethrow or use a `when` filter.
- Validate the arguments of public methods at once (`ArgumentNullException.ThrowIfNull`).
- Release resources with `using`, not by hand in `finally`.
- Do not use exceptions for flow control; use `TryXxx` for expected failures.
- Throw specific types; not `Exception`.
- Do not throw exceptions from `Dispose`, finalizers and `finally`.

## Common mistakes

- ❌ `catch (Exception) { }` → ✅ a specific type, or logging and rethrowing.
- ❌ `throw ex;` → ✅ `throw;`.
- ❌ An exception in `finally` → ✅ code that does not throw; release through `using`.
- ❌ A wrapper without `InnerException` → ✅ pass the cause.
- ❌ `catch (Exception e) when (e.Message.Contains(...))` as a way to tell errors apart → ✅ exception types and properties.
- ❌ Showing `ex.ToString()` to the user → ✅ a generic message, details in the log.
- ❌ Catching `NullReferenceException` instead of checking for `null` → ✅ fix the cause.

## Comparison with other tracks

| | C# | Python |
|---|---|---|
| Syntax | `try`/`catch`/`finally`, `throw` | `try`/`except`/`finally`/`else`, `raise` |
| Filters | `catch … when (condition)` | none, a check inside `except` |
| Rethrowing | `throw;` (preserves the stack) | `raise` without arguments |
| Chain of causes | `InnerException` | `raise X from e`, `__cause__` |
| Releasing resources | `using` | `with` (context managers) |
| Checked exceptions | no | no |

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert](expert.en.md) — two-pass handling, how the stack works, reference, interview questions
- `using` and `Dispose`: [12 · Memory and GC](../12-memory-and-gc/advanced.en.md)
- Exceptions in tasks: 10 · Async/await
- Multithreading and unhandled exceptions in threads: [18 · Concurrency](../18-concurrency/basics.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md)

## Sources

- [Exception filters](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/exception-handling-statements#exception-filters)
- [Best practices for exceptions](https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Creating user-defined exceptions](https://learn.microsoft.com/dotnet/standard/exceptions/how-to-create-user-defined-exceptions)
- [ExceptionDispatchInfo](https://learn.microsoft.com/dotnet/api/system.runtime.exceptionservices.exceptiondispatchinfo)
- [AggregateException](https://learn.microsoft.com/dotnet/api/system.aggregateexception)
- [Compiler error CS0157](https://learn.microsoft.com/dotnet/csharp/misc/cs0157)
- [AppDomain.UnhandledException](https://learn.microsoft.com/dotnet/api/system.appdomain.unhandledexception)
