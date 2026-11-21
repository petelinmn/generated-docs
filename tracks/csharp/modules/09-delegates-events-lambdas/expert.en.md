---
title: "Delegates, events, lambdas: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 09-delegates-events-lambdas
part: 1
tags: [delegate-internals, multicast-delegate, display-class, lambda-caching, event-accessors, function-pointers, closures, delegate-equality, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Delegates, events, lambdas: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert**

## Under the hood

### What a delegate is

Every delegate type declared with `delegate` (and `Func`, `Action` and others) is a **sealed class** that inherits from `System.MulticastDelegate` (verified: the base type of `Action` is `MulticastDelegate`, `IsSealed == True`). A delegate object stores two values:

- the **target** (`Target`) — the object on which the method is called; for a static method and for a non-capturing lambda there is none, or it is an instance of a helper class;
- the **method** (`Method`) — a reference to the implementation.

Verified: for a delegate of the static method `Multiply` — `Method.Name == "Multiply"` and `Target == null`. The `Invoke` method is declared by the compiler and implemented by the runtime itself: a call `d(x)` becomes `d.Invoke(x)`, that is, an indirect call through a reference to the method rather than an ordinary call.

### Multicast

A `MulticastDelegate` holds an **invocation list**. `+=` calls `Delegate.Combine`, `-=` calls `Delegate.Remove`: both **create a new delegate** (the old ones do not change, which makes delegates safe for multithreaded reading). `GetInvocationList()` returns an array of single-method delegates; verified: `Combine(act, act)` for a delegate of three methods gave six. `Remove` looks for the **last occurrence**. The `==` comparison is by method and target: two calls `(Action)Hello` are equal, while two lambdas identical in text are not (different target objects or different methods).

### A non-capturing lambda and the cache

For a non-capturing lambda the compiler creates a static method in a helper class `<>c` (verified: `Target` is an instance of `<>c`, `Method.DeclaringType` is `<>c`) and **caches the delegate** in a static field. That is why `ReferenceEquals(Make(), Make())` is `True` and evaluating the lambda again allocates nothing (0 bytes). Since C# 11 method groups of static methods are cached the same way (0 bytes — verified). For an **instance** method the delegate is bound to the object, so each conversion of a method group creates a new delegate (56 bytes).

### The closure and the hidden class

For a capturing lambda the compiler creates a **closure class** `<>c__DisplayClass…` nested in the type that declares the method (verified: `Target.GetType().Name`, and the `DeclaringType` is `Program`). The captured local variables and parameters become **fields** of this class; the lambda itself is its method. Important consequences:

- All lambdas of one scope use **one** closure object. Verified: in a method where one lambda used a small variable `small` and another a large array `big`, the live first lambda kept `big` alive too — the array was not collected (`IsAlive == True`), although the lambda that used it was already unreachable. So long-lived handlers are better created in a separate method with as little captured as possible.
- Each entry into a scope creates a new closure object (for variables inside a loop — on every iteration; for a `for` variable — one for the whole loop: hence `3,3,3`, see [07 · LINQ](../07-linq/advanced.en.md#closures-the-variable-is-read-at-execution-time)).
- In a file with top-level statements all local variables are in one scope, and the closure therefore carries them all (verified: fields `log, counter, rp, h, d3, fact`).
- `this` is captured as a field if the lambda uses instance members: then the object lives as long as the delegate.

### An event is a field and two methods

For the declaration `public event EventHandler<ValueEventArgs>? Changed;` the compiler generates (verified through reflection on `Publisher`):

| Member | What it is |
|---|---|
| a private field `Changed` of type `EventHandler`1` | stores the delegate |
| the method `add_Changed` | subscribing: `+=` |
| the method `remove_Changed` | unsubscribing: `-=` |
| the event `Changed` | metadata linking `add`/`remove` |

Only `add` and `remove` are visible from outside, so an event cannot be assigned or raised (CS0070). Inside the class the name `Changed` refers to the field, so `Changed?.Invoke(...)` works. The accessors of field-like events are generated thread-safe (by the specification: the field is updated with a compare-and-swap loop). If you write `add`/`remove` by hand, thread safety is up to you.

### Function pointers

Unsafe code has the type `delegate*<…>`: a "bare" pointer to a method with no delegate object. Verified: `delegate*<int, int> p = &Sq; p(4)` returned `16`, and the pointer size is 8 bytes (the same as `IntPtr` on this platform).

| | a delegate | a function pointer |
|---|---|---|
| a heap object | yes (56 bytes) | no |
| target and closure | yes | no, only static methods |
| multicast | yes | no |
| safety | managed | `unsafe`, needs `AllowUnsafeBlocks` |
| use | ordinary code | native interop, high-performance paths |

## Reference

> ⚠️ Verify: the language versions in the lambda forms table and the version since which method groups of static methods are cached (C# 11) are given from memory, not from the official documentation; the forms themselves work on C# 15 (verified by running). The statement about thread-safe accessors of field-like events is based on the language specification and was not verified by running.

### Standard delegate types

| Type | Signature |
|---|---|
| `Action`, `Action<T1…T16>` | `void (…)` |
| `Func<TResult>`, `Func<T1…T16, TResult>` | `TResult (…)` |
| `Predicate<T>` | `bool (T)` |
| `Comparison<T>` | `int (T, T)` |
| `Converter<TIn, TOut>` | `TOut (TIn)` |
| `EventHandler`, `EventHandler<T>` | `void (object? sender, T e)` |
| `AsyncCallback`, `ThreadStart`, `ParameterizedThreadStart` | legacy of threading and asynchronous APIs |

### Lambda and method group forms

| Form | Example | Since |
|---|---|---|
| a lambda expression | `x => x * 2` | C# 3 |
| a lambda with a block | `x => { return x * 2; }` | C# 3 |
| asynchronous | `async x => await Foo(x)` | C# 5 |
| no parameters | `() => 1` | C# 3 |
| discard parameters | `(_, _) => 0` | C# 9 |
| `static` | `static x => x` | C# 9 |
| natural type, `var` | `var f = (int x) => x;` | C# 10 |
| an explicit return type | `int (int x) => x` | C# 10 |
| attributes on a lambda | `[Obsolete] (int x) => x` | C# 10 |
| default parameters and `params` | `(int x, int y = 10) => x + y` | C# 12 |
| a method group | `Func<int,int> f = Twice;` | C# 1 |

### What a closure captures

| Captured | How |
|---|---|
| a local variable, a parameter | a field of the closure class; changes are visible to both sides |
| `this` | a reference field to the object |
| `ref` parameters, `ref struct`, `Span<T>` | **cannot** |

### Delegate members

| Member | Purpose |
|---|---|
| `Target`, `Method` | the target and the method |
| `Invoke(…)` | a call (the same as `d(…)`) |
| `GetInvocationList()` | an array of single-method delegates |
| `DynamicInvoke(object[])` | a call without a compile-time signature check (slow; wrong arguments give `ArgumentException`) |
| `Delegate.Combine`, `Delegate.Remove` | the same as `+`, `-` |
| `Delegate.CreateDelegate(type, method)` | creating a delegate through reflection (`Twice` → the call `d(4)` gave `8`) |

## Gotchas

Every gotcha was verified by running it.

### An exception interrupts a multicast call

`1 → throw → 3`: the third handler did not run. For an independent call use `GetInvocationList()` and a `try` for each.

### The result is the last handler's

A `Func<int>` with `1, 2, 3` returned `3`; the values of the first two are lost.

### Unsubscribing with another copy of a lambda does not work

`combined -= l2` (a lambda with the same text) left the handler: the output is still `lam`.

### A copy of a delegate does not see changes

`Action copy = orig; orig += …; copy();` called only the original method: `+=` creates a new delegate.

### A closure keeps extra objects alive

A live lambda that uses only `small` did not let `big`, captured by another lambda, be collected: they share the closure object.

### An instance method group creates a delegate every time

`Func<int> g = o.Inst;` in a loop — 56 bytes per iteration; for a static method — 0 (cache).

### Calling a null delegate

`n!()` for `null` — `NullReferenceException`; `n?.Invoke()` is safe.

### Events cannot be raised from outside

`c.Evt();` and `c.Evt = null;` outside the class — CS0070; for a public delegate field both operations would be allowed.

### A static lambda and capturing

`static () => x` when it uses a local `x` — CS8820.

### Unsubscribing during a call is safe

The first handler unsubscribed itself in `Set(1)`; the second dispatch called only the second (`once other other`): a copy of the list at the moment the call started is invoked.

### Two identical lambda expressions are not equal

`(Func<int>)(() => 1) == (Func<int>)(() => 1)` — `False`: different methods.

## Trade-offs and alternatives

| Task | Option | Price |
|---|---|---|
| Passing behavior | a delegate (`Func`/`Action`) | an allocation on capture, an indirect call |
| | an interface with one method | more code, but an explicit contract and no closures |
| Notifications | `event` | subscribers are held by the publisher; unsubscribing is needed |
| | an observer interface, `IObservable<T>` | more boilerplate |
| | `Channel<T>` | a different model: a message queue |
| A hot path | a `static` lambda, a delegate created once | pass state as parameters |
| | a `delegate*` pointer | `unsafe`, static methods only |
| Dynamic invocation | `DynamicInvoke` | slow, errors at run time |

## Interview questions

**1. What is a delegate? How is it built?**

<details><summary>Answer</summary>

A type-safe reference to a method. It is a sealed class derived from `MulticastDelegate` that stores a target (`Target`), a method (`Method`) and an invocation list. A call `d(x)` is `d.Invoke(x)`.

</details>

**2. How do `Func`, `Action` and `Predicate` differ?**

<details><summary>Answer</summary>

`Action` returns `void`; `Func<…, TResult>` returns a value (the last type parameter is the result); `Predicate<T>` is `bool (T)`, that is, a special case of `Func<T,bool>`, historically used in `List<T>.RemoveAll`, `FindAll`.

</details>

**3. What is a multicast delegate? What does a call return and what happens on an exception?**

<details><summary>Answer</summary>

A delegate with an invocation list: `+=` adds, `-=` removes; on a call the methods run in order. The return value is the result of the **last** one (verified: `3` for `1, 2, 3`). An exception interrupts the chain: the following handlers are not called (`1 caught x`).

</details>

**4. How does an event differ from a public delegate field?**

<details><summary>Answer</summary>

An event is a delegate field plus `add`/`remove` methods: from outside you can only subscribe and unsubscribe, and only the class itself can raise and overwrite it (CS0070). A public field lets anyone invoke the delegate, replace the chain or null it.

</details>

**5. How do you raise an event correctly? Why `?.Invoke`?**

<details><summary>Answer</summary>

`Changed?.Invoke(this, e)`: if there are no subscribers the event is `null`; `?.` reads the field once and calls the copy, so another thread cannot null it between the check and the call. A direct call of a `null` delegate is `NullReferenceException` (verified).

</details>

**6. What is a closure? What is captured — the value or the variable?**

<details><summary>Answer</summary>

A lambda that uses local variables of the outer method. The **variable** is captured, not the value: the compiler moves it into a field of a hidden class (`<>c__DisplayClass`), and the lambda and the outer code see the same field (`counter == 2` after two calls of `inc`).

</details>

**7. Trick question: what does the code print?**

```csharp
var fs = new List<Func<int>>();
for (int i = 0; i < 3; i++) fs.Add(() => i);
foreach (var f in fs) Console.Write(f() + " ");
```

<details><summary>Answer</summary>

`3 3 3`: a `for` loop has one variable `i`, and the lambdas read its final value. With `foreach (var i in …)` it would be `0 1 2`. The fix is a copy in a local variable inside the loop body.

</details>

**8. When does a lambda allocate memory?**

<details><summary>Answer</summary>

A non-capturing lambda — no (the delegate is cached in a static field, 0 bytes); a capturing lambda — yes: a closure object (24 bytes) and a delegate (56) = 80 bytes per creation (verified). A method group of a static method — 0, of an instance method — 56 per conversion. Invoking an already created delegate does not allocate.

</details>

**9. What does the `static` modifier on a lambda do?**

<details><summary>Answer</summary>

It forbids capturing local variables and `this` (CS8820), so the lambda is guaranteed to have no closure and is cached. It is used to protect against accidental allocations in hot code.

</details>

**10. How do you unsubscribe from an event you subscribed to with a lambda?**

<details><summary>Answer</summary>

Store the delegate in a variable and use it in both `+=` and `-=`. Another lambda with the same text is a different delegate (equality by method and target), and `-=` will not remove it (verified). Either subscribe with a method or return an `IDisposable` token from the subscription method.

</details>

**11. What memory leak do events cause?**

<details><summary>Answer</summary>

The delegate in the publisher's event references the subscriber object. If the publisher lives longer, the subscriber is not collected. Solutions: `-=` in `Dispose`, a subscription token, weak events. More in [12 · Memory and GC](../12-memory-and-gc/advanced.en.md#memory-leaks-in-managed-code).

</details>

**12. Trick question: why does a live lambda keep a large object it does not use from being collected?**

<details><summary>Answer</summary>

All lambdas of one scope share one closure object with fields for **all** captured variables. If one lambda uses `big` and another `small`, and the second one lives, it holds the closure object and therefore `big` too (verified: the array was not collected).

</details>

**13. How are delegates variant?**

<details><summary>Answer</summary>

Covariant in the result (`Func<string>` → `Func<object>`) and contravariant in parameters (`Action<object>` → `Action<string>`); it works for reference types. Verified by assignments.

</details>

**14. How does a delegate differ from a `delegate*` function pointer?**

<details><summary>Answer</summary>

A delegate is a heap object (56 bytes) with a target and an invocation list, managed and safe. `delegate*<…>` is a "bare" pointer (8 bytes on a 64-bit platform) to a static method: no object, target or multicast, only in `unsafe` code; used for native calls and hot paths.

</details>

**15. What is `async void` in event handlers and why is it dangerous?**

<details><summary>Answer</summary>

An event expects a `void` handler, so `async void` is the only acceptable use. An exception from it cannot be caught around the event call: it goes to the synchronization context and may terminate the process. So the body is wrapped in `try/catch`. More in 10 · Async/await.

</details>

**16. How do you call all handlers of a multicast delegate independently of each other?**

<details><summary>Answer</summary>

Iterate `GetInvocationList()` and call each one in its own `try/catch`; then one's exception does not interrupt the others. Results and errors are collected separately.

</details>

## Cheat sheet

| Need | Solution |
|---|---|
| Pass behavior | `Func`/`Action`/`Predicate` |
| Call optionally | `handler?.Invoke(...)` |
| Publish notifications | `event EventHandler<T>` + `protected virtual OnXxx` |
| Unsubscribe from a lambda | store the delegate in a variable |
| A lambda without allocations | a `static` lambda |
| Call all without stopping on an error | `GetInvocationList()` + `try` |
| A recursive lambda | declare the variable beforehand |
| A dynamic call | `DynamicInvoke` (slow) |
| Native paths | `delegate*` in `unsafe` |

## Related materials

- Before: [basics](basics.en.md), [advanced](advanced.en.md)
- Closures and lazy queries: [07 · LINQ](../07-linq/expert.en.md)
- Leaks and releasing: [12 · Memory and GC](../12-memory-and-gc/expert.en.md)
- Exceptions in chains: [08 · Exceptions](../08-exceptions/expert.en.md)
- Reflection and `CreateDelegate`: 20 · Reflection and source generators

## Sources

- [Delegates](https://learn.microsoft.com/dotnet/csharp/programming-guide/delegates/)
- [Events](https://learn.microsoft.com/dotnet/csharp/programming-guide/events/)
- [Lambda expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Specification: anonymous functions and closures](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/expressions#1214-anonymous-function-expressions)
- [Function pointers](https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers)
- [MulticastDelegate](https://learn.microsoft.com/dotnet/api/system.multicastdelegate)
- [Delegate](https://learn.microsoft.com/dotnet/api/system.delegate)
