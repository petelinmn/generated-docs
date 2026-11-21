---
title: "Quiz: Memory and GC"
lang: en
track: csharp
kind: module
doc: quiz
slug: 12-memory-and-gc
part: 2
tags: [gc, generations, large-object-heap, idisposable, finalizer, memory-leak, object-lifetime]
status: draft
updated: 2026-11-21
---

# Quiz: Memory and GC

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 When can an object be collected by the garbage collector?

- A) when its reference count drops to zero
- B) when it is unreachable from the roots: thread stacks, static fields, GC handles
- C) right after the method that created it returns
- D) only after `null` is assigned to every variable that referenced it

<details><summary>Answer</summary>

**B.** The .NET GC does not count references; it walks the graph from the roots, and everything unreachable is garbage. A describes reference counting (that is how CPython works). C is wrong: an object returned from a method or stored in a field keeps living. D is wrong: assigning `null` is neither required nor a guarantee — an object is collected when nothing can reach it, and only at the next collection of its generation.
→ [Basics](basics.en.md#how-the-gc-decides-an-object-is-no-longer-needed)

</details>

## 2. 🟢 Objects `a` and `b` reference each other, and nothing else references them. What does the GC do?

- A) nothing: each has a reference, so both are alive
- B) collects both
- C) collects one of them
- D) throws a circular reference exception

<details><summary>Answer</summary>

**B.** The GC looks for objects reachable from the roots, and the cycle is unreachable. Verified: after `GC.Collect()` a weak reference to an object in the cycle became invalid. Leaks through cycles come from reference counting without a separate cycle collector, not from the .NET GC.
→ [Basics](basics.en.md#how-the-gc-decides-an-object-is-no-longer-needed)

</details>

## 3. 🟢 What does this code print?

```csharp
var survivor = new object();
var generations = new List<int> { GC.GetGeneration(survivor) };
for (int i = 0; i < 3; i++)
{
    GC.Collect();
    generations.Add(GC.GetGeneration(survivor));
}
Console.WriteLine(string.Join(", ", generations));
```

- A) `0, 0, 0, 0`
- B) `0, 1, 2, 3`
- C) `0, 1, 2, 2`
- D) `2, 2, 2, 2`

<details><summary>Answer</summary>

**C.** A new object is in generation 0; each collection it survives promotes it by one generation. There is no generation 3: `GC.MaxGeneration == 2`.
→ [Basics](basics.en.md#generations)

</details>

## 4. 🟢 What does `GC.GetGeneration(new byte[100_000])` return right after the array is created?

- A) `0`
- B) `1`
- C) `2`
- D) `-1`: the array is not on the heap yet

<details><summary>Answer</summary>

**C.** Objects of 85,000 bytes and up are placed in the large object heap (LOH), which is collected together with generation 2, so `GetGeneration` returns `2` right away. The boundary includes the header: on a 64-bit runtime `byte[84_976]` is already a large object.
→ [Basics](basics.en.md#generations)

</details>

## 5. 🟢 In what order will the lines appear?

```csharp
{
    using var a = new Logged("a", log);
    using var b = new Logged("b", log);
    log.Add("body end");
}
log.Add("after block");
```

`Logged.Dispose` adds `dispose <name>` to the log.

- A) `dispose a, dispose b, body end, after block`
- B) `body end, dispose a, dispose b, after block`
- C) `body end, dispose b, dispose a, after block`
- D) `body end, after block`: using declarations are disposed at the end of the method

<details><summary>Answer</summary>

**C.** A using declaration releases the resource at the end of the enclosing block — here the braces, not the method. Several resources are released in reverse order: `b` was created later and may depend on `a`.
→ [Basics](basics.en.md#idisposable-and-using)

</details>

## 6. 🟢 Which statements about `Dispose` are true? (several answers)

- A) `Dispose` releases resources such as files and connections — at the moment it is called
- B) `Dispose` immediately frees the memory of the object itself
- C) `using` calls `Dispose` even if the block threw an exception
- D) the GC calls `Dispose` on unreachable objects by itself

<details><summary>Answer</summary>

**A, C.** `Dispose` closes the resource deterministically, and the object's memory is freed later by the GC (B is wrong). `using` compiles to `try/finally`. The GC calls a finalizer, not `Dispose`, and only if there is one (D is wrong).
→ [Basics](basics.en.md#idisposable-and-using)

</details>

## 7. 🟡 A class has a finalizer, and `Dispose` calls `GC.SuppressFinalize(this)`. The object was released through `using`, then became unreachable, and a collection ran. What happens?

- A) the finalizer runs, but later
- B) the finalizer does not run; the object is collected normally
- C) the finalizer runs twice: from `Dispose` and from the GC
- D) the object is never collected

<details><summary>Answer</summary>

**B.** `SuppressFinalize` removes the object from the finalization queue, so it is collected as an ordinary object in a single collection. Verified: without `Dispose` the finalizer ran, with `Dispose` it did not.
→ [Advanced](advanced.en.md#finalizers)

</details>

## 8. 🟡 Find the bug

```csharp
public sealed class PriceView
{
    public PriceView(PriceFeed feed) // feed is a singleton
    {
        feed.PriceChanged += (_, price) => LastPrice = price;
    }

    public decimal LastPrice { get; private set; }
}
```

<details><summary>Answer</summary>

A memory leak. The lambda captures `this`, the delegate is stored in the singleton's event, and every `PriceView` created lives as long as the `PriceFeed` — even after its window is closed. You cannot unsubscribe from this lambda: the delegate reference is not stored anywhere. The fix: a handler method and unsubscribing in `Dispose` (`IDisposable`), as in the `Ex08_EventLeaks` example: without unsubscribing the subscriber survived a collection, with it — it did not.
→ [Advanced](advanced.en.md#memory-leaks-in-managed-code)

</details>

## 9. 🟡 What does `disposing == false` mean in the `Dispose(bool disposing)` pattern?

- A) the object is already disposed, nothing to do
- B) the call came from a finalizer: only unmanaged resources may be released
- C) the call came from `using`
- D) releasing should be deferred until the next collection

<details><summary>Answer</summary>

**B.** `Dispose()` calls `Dispose(true)`, the finalizer calls `Dispose(false)`. In a finalizer other managed objects may already be finalized, so they must not be touched. A repeated call (A) is filtered out by a separate `_disposed` flag.
→ [Advanced](advanced.en.md#the-disposebool-pattern-for-a-hierarchy)

</details>

## 10. 🟡 How many bytes does adding 1000 `int` values to an `ArrayList` and to a `List<int>` allocate, if both capacities are set in advance? (64-bit runtime)

- A) 0 and 0
- B) 4,000 and 4,000
- C) 24,000 and 0
- D) 24,000 and 4,000

<details><summary>Answer</summary>

**C.** `ArrayList` stores `object`, so every value is boxed — a separate 24-byte heap object each. `List<int>` stores the values directly in its array, and the array is already allocated with the needed capacity. Measured with `GC.GetAllocatedBytesForCurrentThread()`.
→ [Advanced](advanced.en.md#measuring-allocations)

</details>

## 11. 🟡 A class implements only `IAsyncDisposable`. What happens to the code `using (var x = new OnlyAsync()) { }`?

- A) `DisposeAsync` is called but not awaited
- B) a compile error
- C) `Dispose` is not called, the resource leaks
- D) a runtime exception

<details><summary>Answer</summary>

**B.** Error CS8418: the type in a `using` statement must implement `IDisposable`; the compiler suggests `await using`. The opposite — `await using` on a type with only `IDisposable` — is an error too (CS8417). A class that is released asynchronously should implement both interfaces.
→ [Advanced](advanced.en.md#iasyncdisposable-and-await-using)

</details>

## 12. 🟡 A class wraps a file handle from a native library. Does it need a finalizer?

<details><summary>Answer</summary>

Not its own. The handle is wrapped in a `SafeHandle` subclass that overrides `ReleaseHandle`. `SafeHandle` already has a reliable finalizer, release exactly once (verified: a double `Dispose` closed the handle once), and protection against closing the handle in the middle of a native call. The class itself stores the `SafeHandle` and implements a simple `Dispose` that releases it.
→ [Advanced](advanced.en.md#safehandle-instead-of-a-finalizer)

</details>

## 13. 🔴 What does this code print in a Debug build?

```csharp
var obj = new object();
var weak = new WeakReference(obj);
obj = null;
GC.Collect();
Console.WriteLine(weak.IsAlive);
```

- A) `False`: no references are left
- B) `True`
- C) it depends on the object's generation
- D) `NullReferenceException`

<details><summary>Answer</summary>

**B.** In Debug the JIT extends the lifetime of values to the end of the method, and unoptimized code keeps a copy of the reference in a temporary slot, so `obj = null` does not make the object unreachable. Verified: `True` in Debug and on the first call in Release, `False` only in optimized code. That is why tests of GC behavior create the object in a separate method marked `NoInlining`.
→ [Expert](expert.en.md#clearing-a-variable-does-not-free-the-object-in-debug)

</details>

## 14. 🔴 What is printed?

```csharp
try
{
    using var t = new Throwing();                     // Dispose throws "from Dispose"
    throw new InvalidOperationException("from body");
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
```

- A) `from body`
- B) `from Dispose`
- C) `One or more errors occurred.` — an `AggregateException` with both exceptions
- D) nothing: the process crashes

<details><summary>Answer</summary>

**B.** `using` is `try/finally`. An exception from `Dispose` in the `finally` block replaces the exception from the body, and the original cause is lost. That is why `Dispose` must not throw.
→ [Expert](expert.en.md#an-exception-in-dispose-replaces-the-original)

</details>

## 15. 🔴 The app was started with `DOTNET_GCHeapHardLimit=10000000`. What is the managed heap limit?

- A) 10,000,000 bytes (about 10 MB)
- B) 268,435,456 bytes (256 MiB)
- C) 10,000,000 KB
- D) the setting is ignored: the limit can be set only in `runtimeconfig.json`

<details><summary>Answer</summary>

**B.** Numbers in GC environment variables are read as hexadecimal: `0x10000000` = 268,435,456. Verified with `GC.GetGCMemoryInfo().TotalAvailableMemoryBytes`. In `runtimeconfig.json` (`System.GC.HeapHardLimit`) numbers are decimal.
→ [Expert](expert.en.md#hexadecimal-numbers-in-environment-variables)

</details>
