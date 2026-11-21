---
title: "Exceptions: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 08-exceptions
part: 1
tags: [two-pass-exception-handling, exception-clauses, stack-trace, rethrow, exception-filters, unhandled-exceptions, exception-cost, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Exceptions: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert**

## Under the hood

### Handling blocks in IL

In a method's metadata the handlers are described by a table of **exception handling clauses**. Verified by reading `MethodBody.ExceptionHandlingClauses`:

| C# construct | Clause kind in IL |
|---|---|
| `try { } catch (E) { }` | `Clause` (catch) |
| `catch (E e) when (condition)` | `Filter` — the condition is compiled into a **separate filter block** |
| `try { } finally { }` | `Finally` |
| `using`, `lock` | `Finally` (the compiler expands them into `try/finally`) |

The CLR also has a `Fault` kind (it runs only on an exception), but C# does not generate it. For `throw;` the compiler emits the `rethrow` instruction, and for `throw ex;` an ordinary `throw` (verified by reading IL): that is exactly why the first form preserves the stack and the second creates it anew.

### Two-pass handling

When an exception is thrown, the runtime works in **two passes**:

1. **Finding a handler.** It walks up the stack looking for a suitable `catch`; on the way it **runs `when` filter blocks**, but not `finally` blocks. The stack is not unwound yet.
2. **Unwinding.** It goes from the throw site to the handler found and runs the `finally` blocks on the way; then the `catch` itself runs.

Verified by the order of calls: for `try { try { throw } finally { "inner finally" } } catch (Exception) when ("filter") { "catch" }` the log is `filter`, `inner finally`, `catch`. Consequences:

- A filter sees the state **at the moment of the throw**: the locals of the frames below are not yet cleaned up, and the `finally` blocks have not run. That is handy for diagnostics and dangerous if the filter relies on the side effects of `finally`.
- If there is no suitable handler, the second pass does not start: an unhandled exception terminates the process (and the `finally` blocks may not run).
- An exception inside a filter is suppressed, and the filter is considered to have returned `false` (verified: the outer `catch` received the original exception, not "from filter").

### What an exception holds

An exception object is an ordinary object of the `Exception` class (or a derived one) on the heap. The stack (`StackTrace`) is filled in **at the moment of the throw**, not of creation: for a `new Exception("x")` that was not thrown, `StackTrace == null` (verified). So creating an exception is cheaper than throwing; a throw collects the stack frames, and unwinding walks through them. A rethrow `throw;` continues the same trace, `throw ex;` starts a new one, and `ExceptionDispatchInfo.Throw()` restores the original one and adds a separator to the trace (verified: one line starting with `---`, none in a plain exception).

Important members: `Message`, `StackTrace`, `InnerException`, `Data` (a dictionary for context), `HResult` (a code for COM/Win32 interoperation; non-zero for `InvalidOperationException` — verified), `Source`, `TargetSite` (the method where the throw happened — verified: the name `Thrower`), `HelpLink`.

### Exceptions that cannot be caught

- **A stack overflow** (`StackOverflowException`) terminates the process: it cannot be caught, which is why recursion is limited by depth.
- **`Environment.FailFast`** terminates the process immediately, without `finally` or finalizers.
- Running out of memory (`OutOfMemoryException`) can formally be caught, but the program's state after it is unreliable.
- An unhandled exception in any thread terminates the process.

### Cost

Throwing and catching is orders of magnitude more expensive than an ordinary return. In the check, 20,000 `throw`/`catch` cycles took ≈37 ms; 20,000 `int.TryParse` calls with a failed parse took ≈0.3 ms (more than a 100-fold difference; the numbers depend on the machine and the runtime version). But the **no-exception path is almost free**: entering a `try` adds no noticeable cost, so "wrapping in a `try`" is safe, while throwing in a hot loop is not.

## Reference

### Constructs

| Construct | Purpose |
|---|---|
| `try { } catch { }` | catch any exception (no object) |
| `catch (E e)` | catch type `E` and its derived types |
| `catch (E) when (cond)` | catch by condition, a filter |
| `finally { }` | code that always runs |
| `throw new E(…)` | throw a new exception |
| `throw;` | rethrow in a `catch`, preserving the stack |
| `x ?? throw new E()` / `cond ? a : throw …` | a `throw` expression |
| `using` / `await using` | release through `finally` |
| `checked { } / unchecked { }` | turns integer overflow checking on/off |

### Validation helper methods

| Method | Throws |
|---|---|
| `ArgumentNullException.ThrowIfNull(x)` | `ArgumentNullException` |
| `ArgumentException.ThrowIfNullOrEmpty(s)` / `ThrowIfNullOrWhiteSpace(s)` | `ArgumentNullException` for `null`, `ArgumentException` for an empty (or whitespace) string |
| `ArgumentOutOfRangeException.ThrowIfNegative(x)`, `ThrowIfZero`, `ThrowIfGreaterThan(x, max)`, `ThrowIfLessThan` … | `ArgumentOutOfRangeException` |
| `ObjectDisposedException.ThrowIf(cond, this)` | `ObjectDisposedException` |

Verified: `ThrowIfNull((string?)null)`, `ThrowIfNegative(-1)`, `ThrowIfNullOrEmpty("")`, `ThrowIfGreaterThan(5, 3)` threw `ArgumentNullException`, `ArgumentOutOfRangeException`, `ArgumentException`, `ArgumentOutOfRangeException`. The parameter name is taken from the call expression (`CallerArgumentExpression`).

### Typical causes and types

| Situation | Exception (verified) |
|---|---|
| `s.Length` when `s == null` | `NullReferenceException` |
| `a[2]` for `int[1]` | `IndexOutOfRangeException` |
| `list[0]` on an empty `List<int>` | `ArgumentOutOfRangeException` (parameter `index`) |
| `(int)(object)"x"` | `InvalidCastException` |
| `int.Parse("x")` | `FormatException` |
| `Convert.ToInt32("99999999999")` | `OverflowException` |
| `1 / z` when `z == 0` (integers) | `DivideByZeroException`; for `double` the result is infinity |
| `checked(int.MaxValue + 1)` | `OverflowException`; without `checked` the result wraps (`-2147483648`) |
| `dict["zzz"]` | `KeyNotFoundException` |

## Gotchas

Every gotcha was verified by running it.

### throw ex; loses the stack

After `catch (Exception ex) { throw ex; }` the trace started with the catching method and had no `Thrower` frame. `catch { throw; }` kept `Thrower`. The reason is different IL instructions (`throw` and `rethrow`).

### An exception in finally replaces the original

`try { throw A } finally { throw B }` — the caller got `B` without an `InnerException`: the original error is lost.

### The return value is fixed before finally

`try { return x; } finally { x = 99; }` returned `1`. But if a reference to a mutable object is returned, `finally` will change it (the list got 2 elements).

### The filter runs before finally

The order `filter → inner finally → catch`. Code in a filter sees a still "live" state.

### Wait wraps, await unwraps

`Task.Run(() => throw …).Wait()` threw an `AggregateException` with an inner `InvalidOperationException`; an `await` of the same task threw `InvalidOperationException` directly.

### Exceptions in nested using

On an exception in the body of two `using var` the order is `body, dispose b, dispose a, catch` — the resources are released before the catch.

### An exception without a throw has no stack

`new Exception("x").StackTrace` is `null`.

### Division: integer and floating-point

`1 / 0` for `int` is an exception, while `1.0 / 0` is `∞` without an exception.

### A filter that returns false

`catch (Exception e) when (Log(e))` where `Log` returns `false` prints a record and does not catch the exception: a convenient way to log, but the filter must not throw exceptions itself.

### The cost of a throw

20,000 throws ≈ 37 ms versus ≈ 0.3 ms for `TryParse`: exceptions are not suitable for expected results.

## Trade-offs and alternatives

| Task | Option | Price |
|---|---|---|
| Reporting an error | an exception | expensive on throw, but never lost and carries the stack |
| | `TryXxx` with `bool` | the caller must check the result |
| | returning a result (`Result<T>`, `OneOf`) | more boilerplate, but errors are visible in the type |
| Releasing a resource | `using` | — |
| | `try/finally` | more verbose |
| Logging without catching | a `when` filter | a side effect in a condition |
| Adding context | a wrapper with `InnerException` | an extra layer |
| | `ex.Data` | not typed |

C# has no checked exceptions (as in Java): a signature does not declare that a method throws. Document exceptions in XML comments (`<exception cref="…">`).

## Interview questions

**1. How do `try`, `catch` and `finally` work? When does `finally` run?**

<details><summary>Answer</summary>

`try` is the protected code, `catch` is a handler by type, `finally` is code that always runs: after normal completion, after `catch`, on `return`/`break`/`continue` and on an unhandled exception (while the process is alive). It does not run on `Environment.FailFast`, a stack overflow and other abnormal process terminations.

</details>

**2. How does `throw;` differ from `throw ex;`?**

<details><summary>Answer</summary>

`throw;` compiles to `rethrow` and preserves the original stack; `throw ex;` compiles to an ordinary `throw` and starts the trace anew from the current line, losing the place of the real error (verified: there is no `Thrower` frame in the trace). The right one is `throw;`.

</details>

**3. What are exception filters? Why are they better than a check inside `catch`?**

<details><summary>Answer</summary>

`catch (E e) when (cond)` is a condition that determines whether the block handles the exception. If `false`, the search continues and the stack is not unwound: there is no catch and rethrow. A filter runs on the first pass — before the `finally` of inner blocks — so it sees the state at the moment of the throw; it is handy for logging (`when (Log(e))` returning `false`).

</details>

**4. Describe two-pass exception handling.**

<details><summary>Answer</summary>

The first pass looks for a suitable handler up the stack and runs filters; the second pass unwinds the stack, running `finally`, and then runs the `catch` found. Verified: the order `filter → inner finally → catch`. If there is no handler, there is no second pass and the process terminates.

</details>

**5. How does an exception get into `InnerException` and why?**

<details><summary>Answer</summary>

When creating a new exception, the original is passed as the second constructor argument: `new ApplicationException("wrapped", e)`. This adds context or changes the level of abstraction without losing the cause; `ToString()` prints the whole chain.

</details>

**6. What happens if you throw an exception in `finally`?**

<details><summary>Answer</summary>

It replaces the original one: the caller gets only the new one, and `InnerException` is `null` (verified). So there must be no code in `finally` that can throw; use `using` to release resources.

</details>

**7. What does the method `try { return x; } finally { x = 99; }` return with `x = 1`?**

<details><summary>Answer</summary>

`1`: the return value is computed before `finally`. If a reference to a mutable object were returned, `finally` could change its content. `return` inside `finally` is forbidden (error CS0157).

</details>

**8. When are exceptions a bad choice? What to use instead?**

<details><summary>Answer</summary>

For expected cases (user input, a missing key): they are expensive (in the check, 20,000 throws ≈ 37 ms versus ≈ 0.3 ms for `TryParse`). Use the `TryXxx` pattern with `bool` and `out`, `TryGetValue`, returning a result. Exceptions are for contract violations and unforeseen failures.

</details>

**9. How do you create your own exception? What are the rules?**

<details><summary>Answer</summary>

Inherit from `Exception` or a suitable specific type (not `ApplicationException`), name it with the suffix `Exception`, pass the message and `InnerException` to the base constructor, add properties with data. Create one only if the calling code will catch exactly this type.

</details>

**10. How does `AggregateException` differ from an ordinary one? What does `await` do?**

<details><summary>Answer</summary>

It collects several exceptions (`InnerExceptions`) — from `Task.WaitAll`, `Parallel`, `Task.Wait()`. `await` unwraps and throws the **first** original exception (verified: `InvalidOperationException`, while `Wait()` gave `AggregateException`). The `Flatten()` and `Handle()` methods take apart nested ones.

</details>

**11. What is `ExceptionDispatchInfo`?**

<details><summary>Answer</summary>

A class that captures an exception (`Capture`) and later throws it (`Throw()`) preserving the original stack frames — including on another thread or in another method. The propagation of exceptions through `await` is built on it.

</details>

**12. How do exceptions affect performance? Do `try` blocks cost anything?**

<details><summary>Answer</summary>

Throwing and catching are expensive (the stack is collected and unwound). Entering a `try` on the "happy path" is practically free, so `try` in hot code is acceptable while throwing exceptions in a loop is not.

</details>

**13. Trick question: why does `1 / 0` throw an exception while `1.0 / 0` does not?**

<details><summary>Answer</summary>

Integer division by zero has no result and throws `DivideByZeroException`; for floating-point numbers the IEEE 754 standard defines infinity and `NaN` (verified: `∞`).

</details>

**14. What is `OverflowException` and when does it occur?**

<details><summary>Answer</summary>

In integer arithmetic in a `checked` context (`checked(int.MaxValue + 1)`), in `Convert`/`Parse` for values that are too large, in `Enumerable.Sum` for `int`. Without `checked` the arithmetic wraps (`-2147483648`). By default overflow checking for ordinary arithmetic is off; it is turned on by `checked` or a compiler option.

</details>

**15. Which exceptions cannot be caught?**

<details><summary>Answer</summary>

A stack overflow (`StackOverflowException`) terminates the process; `Environment.FailFast` terminates it immediately, without `finally`. After `OutOfMemoryException` the program's state is unreliable. An unhandled exception in any thread also terminates the process.

</details>

**16. Trick question: what does the code print?**

```csharp
try { try { throw new Exception("e"); } finally { Console.Write("inner "); } }
catch (Exception) when (Log()) { Console.Write("catch"); }
// Log() prints "filter " and returns true
```

<details><summary>Answer</summary>

`filter inner catch`: the filter runs on the first pass, before `finally`. If `Log()` returned `false`, the exception would not be caught by this block.

</details>

**17. How do you validate method arguments and which exceptions to throw?**

<details><summary>Answer</summary>

Right at the start of a public method: `ArgumentNullException.ThrowIfNull(x)`, `ArgumentOutOfRangeException.ThrowIfNegative(n)`, `ArgumentException.ThrowIfNullOrEmpty(s)`. For an invalid state — `InvalidOperationException`, for a disposed object — `ObjectDisposedException.ThrowIf`. The parameter name is filled in automatically.

</details>

## Cheat sheet

| Need | Solution |
|---|---|
| Release a resource | `using` |
| Rethrow an exception | `throw;` |
| Wrap with a cause | `new X("…", inner)` |
| Catch by condition | `catch (E e) when (…)` |
| Log without catching | a filter that returns `false` |
| Validate an argument | `ArgumentNullException.ThrowIfNull` etc. |
| An expected error | `TryXxx` |
| Defer a throw | `ExceptionDispatchInfo.Capture(e).Throw()` |
| Several task errors | `AggregateException`, `await` |
| Must not throw | `Dispose`, `finally`, finalizers |

## Related materials

- Before: [basics](basics.en.md), [advanced](advanced.en.md)
- `using`, `Dispose`, finalizers: [12 · Memory and GC](../12-memory-and-gc/expert.en.md)
- Exceptions in `async`: 10 · Async/await
- `checked`/`unchecked` and overflow: [02 · Types and variables](../02-types-and-variables/basics.en.md)
- Methods and iterators: [03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md)

## Sources

- [Exceptions in C#](https://learn.microsoft.com/dotnet/csharp/fundamentals/exceptions/)
- [Best practices for exceptions](https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
- [The CLR exception handling structure (ECMA-335, section I.12.4.2)](https://learn.microsoft.com/dotnet/standard/clr)
- [Exception](https://learn.microsoft.com/dotnet/api/system.exception)
- [ExceptionHandlingClause](https://learn.microsoft.com/dotnet/api/system.reflection.exceptionhandlingclause)
- [Environment.FailFast](https://learn.microsoft.com/dotnet/api/system.environment.failfast)
- [The checked and unchecked statements](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/checked-and-unchecked)
