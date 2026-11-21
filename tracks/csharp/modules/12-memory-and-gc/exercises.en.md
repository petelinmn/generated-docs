---
title: "Exercises: Memory and GC"
lang: en
track: csharp
kind: module
doc: exercises
slug: 12-memory-and-gc
part: 2
tags: [idisposable, dispose-pattern, memory-leak, weak-reference, weak-event]
status: draft
updated: 2026-11-21
---

# Exercises: Memory and GC

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own before opening the hints. Solutions are under spoilers; runnable versions are in [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), and their tests in [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 A temporary file

**Task:** write a `TempFile : IDisposable` class. The constructor creates a temporary file with the given text, the `Path` property returns its path, and `Read()` reads the content. `Dispose` deletes the file. A repeated `Dispose` does nothing, and `Read()` after `Dispose` throws `ObjectDisposedException`.

**Expected result:**

```text
var temp = new TempFile("hello");
temp.Read()                → "hello"
temp.Dispose(); temp.Dispose();
File.Exists(temp.Path)     → false
temp.Read()                → ObjectDisposedException
```

<details><summary>Hint</summary>

`System.IO.Path.GetTempFileName()` creates an empty temporary file and returns its path. A `_disposed` flag makes `Dispose` idempotent, and `ObjectDisposedException.ThrowIf(_disposed, this)` is a short check at the start of methods.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: a temporary file that deletes itself on Dispose.
public sealed class TempFile : IDisposable
{
    private bool _disposed;

    public TempFile(string content)
    {
        Path = System.IO.Path.GetTempFileName();
        File.WriteAllText(Path, content);
    }

    public string Path { get; }

    public string Read()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return File.ReadAllText(Path);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        File.Delete(Path); // does not throw if the file is already gone
    }
}
```

The class is `sealed` and holds only managed resources, so neither the `Dispose(bool)` pattern nor a finalizer is needed. `File.Delete` does not throw if the file is missing — one more safeguard for idempotency.

</details>

## 2. 🟡 A container of disposables

**Task:** write `CompositeDisposable : IDisposable` with an `Add(IDisposable item)` method. `Dispose` releases everything added **in reverse order**. If some `Dispose` throws, the rest are still released, and at the end an `AggregateException` with all errors is thrown. A repeated `Dispose` does nothing. An object added after `Dispose` is released immediately.

**Expected result:**

```text
Add(a); Add(b — throws in Dispose); Add(c)
Dispose() → order: c, b, a; AggregateException with one error "b"
after Dispose: Add(late) → late is disposed immediately
```

<details><summary>Hint</summary>

Walk the list from the end, wrap each `Dispose` in `try/catch` and collect the exceptions. Set the `_disposed` flag before the walk: then a repeated call from some inner `Dispose` will not start the walk again.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: a container that disposes everything it holds — in reverse order, even if some Dispose throws.
public sealed class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _items = [];
    private bool _disposed;

    public void Add(IDisposable item)
    {
        if (_disposed)
        {
            item.Dispose(); // the owner is gone: release the newcomer right away
            return;
        }
        _items.Add(item);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        List<Exception>? errors = null;
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            try
            {
                _items[i].Dispose();
            }
            catch (Exception e)
            {
                (errors ??= []).Add(e);
            }
        }
        _items.Clear();
        if (errors is not null)
        {
            throw new AggregateException(errors);
        }
    }
}
```

Reverse order mirrors several `using var` declarations: what was created later is released earlier. Here `Dispose` throws on purpose — after everything has been released; otherwise one object's error would leave the others unreleased.

</details>

## 3. 🟡 A subscription you can dispose

**Task:** write a `MessageHub` with `Subscribe(Action<string> handler)` and `Publish(string message)` methods. `Subscribe` returns an `IDisposable` whose `Dispose` unsubscribes the handler. A repeated `Dispose` is safe. An unsubscribed subscriber must be collectible by the GC.

**Expected result:**

```text
var token = hub.Subscribe(listener.On);
hub.Publish("one"); token.Dispose(); token.Dispose(); hub.Publish("two");
listener.Received → ["one"], hub.Count → 0
a subscriber unsubscribed through using, after GC.Collect() → collected
```

<details><summary>Hint</summary>

Return from `Subscribe` an instance of a nested class that stores the hub and the handler and removes the handler from the list in `Dispose`. After unsubscribing, null out the hub reference: the token will not keep the hub alive, and a repeated call becomes a no-op.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: subscribing returns a token; disposing the token unsubscribes.
public sealed class MessageHub
{
    private readonly List<Action<string>> _handlers = [];

    public int Count => _handlers.Count;

    public IDisposable Subscribe(Action<string> handler)
    {
        _handlers.Add(handler);
        return new Subscription(this, handler);
    }

    public void Publish(string message)
    {
        foreach (Action<string> handler in _handlers.ToArray())
        {
            handler(message);
        }
    }

    private sealed class Subscription(MessageHub hub, Action<string> handler) : IDisposable
    {
        private MessageHub? _hub = hub;

        public void Dispose()
        {
            _hub?._handlers.Remove(handler);
            _hub = null; // idempotent, and the token no longer keeps the hub alive
        }
    }
}
```

`Publish` walks a copy of the list: a handler may unsubscribe right during publishing, and walking the original list would throw `InvalidOperationException`. Subscriptions in Rx work the same way (`IObservable<T>.Subscribe` returns `IDisposable`).

</details>

## 4. 🔴 A weak event

**Task:** write `WeakEvent<TArgs>` with `Subscribe(Action<TArgs> handler)` and `Raise(TArgs args)` methods and a `Count` property. The event must not keep its subscribers alive: if a subscriber is no longer used anywhere, the GC collects it, and `Raise` removes its entry. Live subscribers receive events in subscription order.

**Expected result:**

```text
live subscriber: Raise(1); Raise(2) → received [1, 2]
forgotten subscriber: GC.Collect() → collected; Raise(1) → Count == 0
```

<details><summary>Hint</summary>

You cannot store the delegate itself: it references the subscriber object (`handler.Target`) strongly. Store a `WeakReference<object>` to `Target` and the `MethodInfo` (`handler.Method`), and call through `method.Invoke(target, [args])`. A static method has `Target == null` — such a delegate can be stored as is.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: an event that does not keep its subscribers alive.
public sealed class WeakEvent<TArgs>
{
    private readonly List<(WeakReference<object>? Target, MethodInfo Method, Action<TArgs>? Static)> _handlers = [];

    public int Count => _handlers.Count;

    public void Subscribe(Action<TArgs> handler)
    {
        if (handler.Target is null)
        {
            _handlers.Add((null, handler.Method, handler)); // static methods have no target to lose
        }
        else
        {
            // Keep only a weak reference to the target, never the delegate itself:
            // the delegate references the target strongly.
            _handlers.Add((new WeakReference<object>(handler.Target), handler.Method, null));
        }
    }

    public void Raise(TArgs args)
    {
        foreach (var (target, method, staticHandler) in _handlers.ToArray())
        {
            if (staticHandler is not null)
            {
                staticHandler(args);
            }
            else if (target!.TryGetTarget(out object? instance))
            {
                method.Invoke(instance, [args]);
            }
        }
        // Subscribers that were collected: clean up their entries.
        _handlers.RemoveAll(h => h.Target is not null && !h.Target.TryGetTarget(out _));
    }
}
```

The trap of weak events is lambdas with closures. For a lambda that captures local variables, `Target` is the closure object, referenced only by the delegate itself. The weak event will not keep it alive, and the subscription "dies" at the next collection even though the subscriber is alive. Verified: the subscription `v => sink.Add(v)` received only the first event and disappeared after `GC.Collect()` (`Count == 0`), even though the `sink` list was still in use. That is why you subscribe with an instance method (`listener.On`), not a lambda. Calling through `MethodInfo.Invoke` is slower than calling a delegate directly — for frequent events the delegate is built once, for example with `CreateDelegate`.

</details>
