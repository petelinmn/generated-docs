---
title: "Exceptions: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 08-exceptions
part: 1
tags: [exceptions, try-catch-finally, throw, custom-exceptions, exception-hierarchy, argument-validation, using, inner-exception]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Exceptions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md)

**Prerequisites:** [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md), [04 · OOP: classes and objects](../04-oop-basics/overview.en.md).

## Why it matters

An operation can fail: the file is missing, an argument is wrong, the network is unavailable. An **exception** is an object that describes such an error and **interrupts the normal flow of execution**: control passes to the nearest handler up the call stack. This frees you from checking a return code after every call and does not let you silently ignore an error: an unhandled exception terminates the program.

## try, catch, finally

```csharp
try
{
    Console.WriteLine("try");
    throw new InvalidOperationException("boom");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"catch: {ex.Message}");
}
finally
{
    Console.WriteLine("finally");
}
```

The output: `try`, then `catch: boom`, then `finally`.

- **`try`** — code that may throw an exception.
- **`catch (Type e)`** — a handler for exceptions of this type (and its derived types). There can be several `catch` blocks.
- **`finally`** — runs **always**: after a `try` without errors, after a `catch`, and even when the exception is not caught or a `return` was executed in the `try`. It is for releasing resources.
- `throw` creates and "throws" an exception: `throw new ArgumentException("message")`.

Verified: for nested `try`/`catch`/`finally` the order is `try catch finally`, then the outer block's `finally`. A method with `return 1` in the `try` and a `finally` returned `1`, and the `finally` still ran.

### Several catch blocks

The blocks are checked from top to bottom, and the **first matching one** runs. That is why more specific types come first:

```csharp
try { throw new ArgumentNullException("p"); }
catch (ArgumentNullException) { Console.WriteLine("specific"); }   // chosen
catch (ArgumentException)     { Console.WriteLine("general"); }
```

If you put a general `catch` above a specific one, the compiler reports an error (CS0160): the specific block is unreachable.

## The exception hierarchy

All exceptions inherit from `System.Exception`. Verified through base type chains:

| Exception | Chain |
|---|---|
| `ArgumentNullException` | → `ArgumentException` → `SystemException` → `Exception` |
| `FileNotFoundException` | → `IOException` → `SystemException` → `Exception` |
| `KeyNotFoundException` | → `SystemException` → `Exception` |
| `DivideByZeroException` | → `ArithmeticException` → `SystemException` → `Exception` |

`catch (Exception)` catches everything, but that is almost always too coarse: catch what you know how to handle.

## Common exceptions

| Exception | Cause |
|---|---|
| `NullReferenceException` | accessing a member through `null` |
| `ArgumentException`, `ArgumentNullException`, `ArgumentOutOfRangeException` | a wrong method argument |
| `InvalidOperationException` | the operation is not valid in the object's current state |
| `IndexOutOfRangeException` | an array index out of bounds |
| `ArgumentOutOfRangeException` | an index out of bounds in `List<T>` (parameter `index`) |
| `KeyNotFoundException` | the key is not in the dictionary |
| `InvalidCastException` | a cast to an incompatible type |
| `FormatException` | a string in the wrong format (`int.Parse("x")`) |
| `OverflowException` | overflow in a `checked` context or `Convert` |
| `DivideByZeroException` | integer division by zero (for `double` — infinity, not an exception) |
| `FileNotFoundException`, `IOException` | files and I/O |
| `ObjectDisposedException` | using an already disposed object |
| `NotImplementedException`, `NotSupportedException` | not implemented / not supported |

The types of all these exceptions were verified by running (for example, `list[0]` on an empty `List<int>` is an `ArgumentOutOfRangeException` with the parameter `index`).

## Exception properties

```csharp
try { Thrower(); }
catch (Exception e)
{
    Console.WriteLine(e.Message);          // boom
    Console.WriteLine(e.GetType().Name);   // InvalidOperationException
    Console.WriteLine(e.StackTrace);       // the call stack at the moment of the throw
    Console.WriteLine(e.InnerException);   // the cause, if any
}
```

- `Message` is a human-readable description; `StackTrace` is the call stack at the moment of `throw`; `InnerException` is the original cause if the exception was wrapped.
- `ToString()` gives the type, the message and the stack: the first line is `System.InvalidOperationException: boom`.
- An exception that was created but **not thrown** has no stack (`StackTrace == null`).

## Argument validation

Public methods validate arguments and throw immediately. There are short helper methods for this:

```csharp
public void Process(string name, int count)
{
    ArgumentNullException.ThrowIfNull(name);
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    ArgumentException.ThrowIfNullOrEmpty(name);
    // ...
}
```

Verified: they throw `ArgumentNullException`, `ArgumentOutOfRangeException` and `ArgumentException` respectively. The name of the checked argument is filled in automatically.

A **`throw` expression** can be used inside an expression:

```csharp
string name = input ?? throw new ArgumentNullException(nameof(input));
```

## using and exceptions

`using` releases a resource even on an exception (it turns into `try/finally`, see [12 · Memory and GC](../12-memory-and-gc/basics.en.md#idisposable-and-using)). Verified: with two `using var` and an exception in the body the order was `body, dispose b, dispose a, catch` — the resources were released before the `catch` handler got control.

## Custom exceptions

When there is no suitable exception, you create your own — inherit from `Exception` (or from a suitable existing type) and add data:

```csharp
public class OrderException(int orderId, string message) : InvalidOperationException(message)
{
    public int OrderId { get; } = orderId;
}

try { throw new OrderException(5, "bad order"); }
catch (OrderException e) { Console.WriteLine($"{e.OrderId} {e.Message}"); }   // 5 bad order
```

The name ends with `Exception`. Custom exceptions are needed when it makes sense for the calling code to catch **exactly this case** and act differently; otherwise use a standard one.

## Wrapping the cause

If you turn one error into another, keep the original in `InnerException`:

```csharp
try { Thrower(); }
catch (Exception e) { throw new ApplicationException("wrapped", e); }
// message: wrapped, inner: boom (InvalidOperationException)
```

## When to throw an exception

- ✅ A method's contract is violated: a wrong argument, an invalid state.
- ✅ The operation cannot fulfil its purpose and the caller would not get a correct result.
- ❌ An ordinary, expected case: the user typed a non-number, a key is missing from a dictionary — there are `TryParse`, `TryGetValue` for those.
- ❌ Flow control (leaving a loop, returning a value): exceptions are slow — in the check, 20,000 `throw`/`catch` took ≈37 ms while 20,000 `int.TryParse` calls took ≈0.3 ms (the numbers depend on the machine; the order of the difference is tens to hundreds of times).

## Beginner mistakes

- ❌ An empty `catch { }` → ✅ handle, log or do not catch: a silently swallowed error hides the cause.
- ❌ `catch (Exception)` everywhere → ✅ catch specific types you know how to react to.
- ❌ `throw ex;` in a `catch` → ✅ `throw;` preserves the original stack (see [advanced](advanced.en.md#throw-and-throw-ex)).
- ❌ Losing the original cause when wrapping → ✅ pass the `InnerException`.
- ❌ Exceptions for expected situations → ✅ `TryParse`/`TryGetValue`.
- ❌ Throwing `Exception` or `ApplicationException` directly → ✅ a specific type.

## Related materials

- Next: [advanced](advanced.en.md) — `when` filters, `throw` and `throw ex`, `AggregateException`, `ExceptionDispatchInfo`, designing your own exceptions
- Releasing resources: [12 · Memory and GC](../12-memory-and-gc/basics.en.md)
- Classes and inheritance: [04 · OOP: classes and objects](../04-oop-basics/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — `try`/`except`/`finally`, `raise`, context managers

## Sources

- [Exception handling](https://learn.microsoft.com/dotnet/csharp/fundamentals/exceptions/exception-handling)
- [Creating and throwing exceptions](https://learn.microsoft.com/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions)
- [The try-catch-finally statement](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/exception-handling-statements)
- [The .NET exception hierarchy](https://learn.microsoft.com/dotnet/standard/exceptions/)
- [Best practices for exceptions](https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
