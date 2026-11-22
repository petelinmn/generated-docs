---
title: "Delegates, events, lambdas: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 09-delegates-events-lambdas
part: 1
tags: [multicast-delegates, closures, static-lambda, delegate-allocation, event-pattern, thread-safety, custom-event-accessors, delegate-variance, local-functions, async-void]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Delegates, events, lambdas: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Multicast delegates

```csharp
Action bad = () => log.Add("1");
bad += () => throw new InvalidOperationException("x");
bad += () => log.Add("3");
try { bad(); } catch (Exception e) { log.Add("caught " + e.Message); }
// 1 caught x — the third handler never ran
```

The rules, verified by running:

- **The order** of invocation is the order of addition.
- **The result** is the value of the last method: for a `Func<int>` with three handlers `1`, `2`, `3` the call returned `3`. The results of the others are lost.
- **An exception interrupts the chain**: after a throw the remaining handlers do not run. To call all of them independently, iterate `GetInvocationList()` and call each one in its own `try`.
- **`+=` on `null`** creates a delegate: `Action? none = null; none += …;` worked. Calling `none()` on an empty delegate gives `NullReferenceException`, hence `?.Invoke()`.
- **Delegates are immutable**: `+=` and `-=` create a **new** delegate. A copy `Action copy = orig;` made before `orig += …` stayed as it was (verified).
- **`-=`** removes the **last occurrence** of an equal delegate: for `all = r1; all += r1; all -= r1;` one `r1` remained. Comparison is by method and target: `(Action)Hello == (Action)Hello` is `True`, while two lambdas identical in text are `False`, so you cannot unsubscribe with an "identical" lambda (verified: it was not removed).
- `Delegate.Combine(a, b)` and `Delegate.Remove(...)` are the same as `+` and `-` (`Combine(act, act)` for a delegate of three methods gave a list of six).

## Lambdas in detail

### Natural type and parameters

```csharp
var natural = (int x) => x + 1;                  // Func<int, int>
var withDefault = (int x, int y = 10) => x + y;  // withDefault(1) == 11
var typed = int (int x) => x;                    // explicit return type
Func<int, int> sq = static x => x * x;           // static lambda
```

A method group also gets a natural type: `var f = Hello;` is `Action` (when a single suitable method exists).

### static lambdas

The `static` modifier forbids a lambda to capture variables (and `this`): error CS8820 "A static anonymous function cannot contain a reference to 'x'" (verified). This protects against accidental closures and extra allocations.

### Closures and their cost

When a lambda uses a local variable, the compiler moves it into a field of a hidden class (`<>c__DisplayClass…`) and makes the lambda a method of that class. Verified through reflection: for the delegate `inc` the `Target` property is an object of the class `<>c__DisplayClass0_0`, and its fields are **all** the captured variables of the scope (`log`, `counter`, …): they live in one object.

Memory allocations (measured with `GC.GetAllocatedBytesForCurrentThread`):

| Expression | Allocated |
|---|---|
| a non-capturing lambda (`x => x + 1`) | 0 bytes: the compiler caches the delegate |
| a method group of a static method | 0 bytes: cached too (verified on .NET 11) |
| a method group of an instance method (`o.Inst`) | 56 bytes: a new delegate is created every time |
| a capturing lambda (`() => y`) | 80 bytes: the closure object (24) + the delegate (56) |
| invoking an already created delegate ×1000 | 0 bytes |

A consequence for hot code: do not create a capturing delegate on every iteration; use `static` lambdas and pass state as a parameter; create the delegate once and reuse it.

```csharp
static Func<int, int> Make() => x => x + 1;            // ReferenceEquals(Make(), Make()) == true: cached
static Func<int> MakeCap(int y) => () => y;            // a new delegate and closure every call
```

### Local functions and lambdas

A local function is also a method inside a method, but it need not be assigned to a delegate, it can have a name, recursion and attributes, and the compiler does not create a delegate until you require one. If you pass it as a delegate (`Func<int,int> f = Local;`) it behaves like a lambda. A recursive lambda requires declaring the variable beforehand: `Func<int,int> fact = null!; fact = n => n <= 1 ? 1 : n * fact(n - 1);` (`fact(5)` is `120`).

### Returning functions

```csharp
Func<int, Func<int, int>> adder = a => b => a + b;   // currying
Console.WriteLine(adder(2)(3));                       // 5
```

Each nested lambda creates its own closure.

## Events in detail

### An event and a public field

```csharp
public class C { public event Action? Evt; }

var c = new C();
c.Evt();          // error CS0070: an event can be only on the left of += and -=
c.Evt = null;     // error CS0070
```

An event lets a subscriber only `+=`/`-=`. A public delegate field lets **anyone** invoke it, replace the chain or null it: foreign code will break the subscriptions.

### The EventArgs pattern

- The publisher declares `event EventHandler<TEventArgs>? Name`.
- The invocation is wrapped in a protected virtual `OnName` method so that derived classes can override it:

```csharp
protected virtual void OnChanged(ValueEventArgs e) => Changed?.Invoke(this, e);
```

- The event data is a class derived from `EventArgs` (in modern .NET `EventHandler<T>` does not require the constraint `T : EventArgs`, but the pattern is still the accepted one).
- Names: an event is a verb or a participle (`Clicked`, `Changed`, `Closing`); the raising method is `On` + the name.

### Thread safety of the invocation

The check `if (Changed != null) Changed(...)` is unsafe: between the check and the call another thread can unsubscribe the last handler. The expression `Changed?.Invoke(...)` reads the field **once** and calls the copy, so `NullReferenceException` cannot occur. Because delegates are immutable, unsubscribing during a call is safe too: in the check the first handler unsubscribed itself during `Set(1)`, and on the second `Set(2)` only the second one was called (the log `once other other`). But the handlers themselves run on the caller's thread and must be ready for multithreading.

### Custom add/remove accessors

By default an event is a delegate field plus `add`/`remove` methods (verified through reflection: `Publisher` has `add_Changed`, `remove_Changed`, an event `Changed` and a private field `Changed` of type `EventHandler`1`). The accessors can be written by hand, for example to keep subscribers in a dictionary or to check a subscription:

```csharp
private Action? _h;
public event Action Evt
{
    add { _h += value; }
    remove { _h -= value; }
}
```

### Leaks and unsubscribing

A `+=` subscription stores a reference to the subscriber object in the publisher's delegate. If the publisher lives longer, the subscriber will not be collected: in detail with an example in [12 · Memory and GC](../12-memory-and-gc/advanced.en.md#memory-leaks-in-managed-code). The solutions: `-=` in `Dispose`, returning an `IDisposable` token from the subscription method, weak events.

### async void and events

An event handler can be `async void` (the only justified use of `async void`): an exception from it cannot be caught around the event call, it goes to the synchronization context and may terminate the process (10 · Async/await). Wrap the handler body in `try/catch`.

## Delegate variance

Delegates are variant in parameter and result types:

```csharp
Func<string> fs = () => "s";
Func<object> fo = fs;                        // covariance by result

Action<object> ao = o => Console.Write("ao");
Action<string> asx = ao;                     // contravariance by parameter
asx("x");
```

Verified: both assignments work. It works only for reference types. More on the rules in [06 · Collections and generics](../06-collections-and-generics/advanced.en.md#variance).

## Invoking without compiler checks

`Delegate.DynamicInvoke(params object[])` calls a delegate of unknown signature: `d3.DynamicInvoke(1, 2)` gave `3`, and wrong arguments (`"x"`, `2`) gave `ArgumentException` at run time. It is noticeably slower than an ordinary call; use it only when the type is unknown at compile time.

## Best practices

- Use `Func`/`Action`/`Predicate` instead of your own delegates; your own one when you need `ref`/`out`/`in` parameters or a meaningful name.
- An event is `EventHandler<T>` with a protected `OnXxx`; raise it through `?.Invoke`.
- Unsubscribe from events of long-lived objects.
- In hot code use `static` lambdas and delegates created once.
- Do not rely on the order in which subscribers run and do not throw exceptions from handlers: the first is not guaranteed and the second interrupts the chain.
- For a public API prefer events or interfaces over multicast delegates with a result.

## Common mistakes

- ❌ An exception in one handler and the rest are not called → ✅ iterate `GetInvocationList()` with a `try` for each.
- ❌ Unsubscribing with an "identical" lambda → ✅ store the delegate in a variable or use a method.
- ❌ A capturing lambda in a loop that runs millions of times → ✅ `static` and a state parameter.
- ❌ `async void` outside event handlers → ✅ `async Task`.
- ❌ A public delegate field → ✅ `event`.
- ❌ A `!= null` check and a call separately → ✅ `?.Invoke`.

## Comparison with other tracks

| | C# | Python |
|---|---|---|
| A reference to a function | a delegate (typed by a signature) | a function is an object, the `Callable` type in annotations |
| An anonymous function | a lambda: an expression or a block | `lambda`: an expression only |
| Closures | capture the variable | capture the variable (`nonlocal`) |
| Events | `event`, `EventHandler<T>` | no built-in ones: callbacks, signal libraries |
| Several subscribers | multicast delegates | a list of callbacks by hand |

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert](expert.en.md) — how delegates and events work, function pointers, reference, interview questions
- Closures in queries: [07 · LINQ](../07-linq/advanced.en.md)
- Leaks through events: [12 · Memory and GC](../12-memory-and-gc/advanced.en.md)
- Exceptions in handler chains: [08 · Exceptions](../08-exceptions/advanced.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md)

## Sources

- [Multicast delegates](https://learn.microsoft.com/dotnet/csharp/programming-guide/delegates/how-to-combine-delegates-multicast-delegates)
- [Static anonymous functions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions#static-anonymous-functions)
- [Natural type of a lambda expression](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions#natural-type-of-a-lambda-expression)
- [How to publish events that conform to .NET guidelines](https://learn.microsoft.com/dotnet/csharp/programming-guide/events/how-to-publish-events-that-conform-to-net-framework-guidelines)
- [Variance in delegates](https://learn.microsoft.com/dotnet/csharp/programming-guide/concepts/covariance-contravariance/using-variance-in-delegates)
- [Delegate.DynamicInvoke](https://learn.microsoft.com/dotnet/api/system.delegate.dynamicinvoke)
- [Local functions](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/local-functions)
