---
title: "Exercises: Delegates, events, lambdas"
lang: en
track: csharp
kind: module
doc: exercises
slug: 09-delegates-events-lambdas
part: 1
tags: [delegates, lambdas, events, closures, multicast, idisposable]
status: draft
updated: 2026-11-22
---

# Exercises: Delegates, events, lambdas

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Function composition

**Task:** write `Compose(Func<int,int> f, Func<int,int> g)` that returns a function "first `f`, then `g`". Apply it to `inc = x + 1` and `dbl = x * 2`.

**Expected result:**

```text
Compose(inc, dbl)(5) → 12;  Compose(dbl, inc)(5) → 11
[1, 2, 3].Select(Compose(inc, dbl)) → 4,6,8
```

<details><summary>Hint</summary>

The method returns a lambda that captures `f` and `g`: `x => g(f(x))`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: a function that returns a function (the lambda captures f and g).
static Func<int, int> Compose(Func<int, int> f, Func<int, int> g) => x => g(f(x));

var inc = (int x) => x + 1;
var dbl = (int x) => x * 2;
Console.WriteLine($"{Compose(inc, dbl)(5)} {Compose(dbl, inc)(5)}");               // 12 11
Console.WriteLine(string.Join(",", new[] { 1, 2, 3 }.Select(Compose(inc, dbl))));  // 4,6,8
```

Verified: `12 11` and `4,6,8`. The argument order matters — composition is not commutative.

</details>

## 2. 🟢 A button event

**Task:** a class `Button` with an event `Clicked` of the standard form `EventHandler<ClickArgs>` (`ClickArgs` holds a `Label`) and a method `Click(string label)`. Subscribe with a lambda, click twice and show the click count and the last label.

**Expected result:**

```text
Click("OK"), Click("Cancel") → 2 Cancel
```

<details><summary>Hint</summary>

Raise the event with `Clicked?.Invoke(this, new ClickArgs(label))`: there may be no subscribers.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: the standard event pattern.
class ClickArgs(string label) : EventArgs { public string Label { get; } = label; }

class Button
{
    public event EventHandler<ClickArgs>? Clicked;
    public void Click(string label) => Clicked?.Invoke(this, new ClickArgs(label));
}

var btn = new Button(); int clicks = 0; string last = "";
btn.Clicked += (s, e) => { clicks++; last = e.Label; };
btn.Click("OK"); btn.Click("Cancel");
Console.WriteLine($"{clicks} {last}");   // 2 Cancel
```

Verified: `2 Cancel`. From outside the class an event cannot be raised — only subscribed to.

</details>

## 3. 🟡 Calling all handlers

**Task:** a multicast `Action` consists of three handlers, the middle one throws. Write `InvokeAll(Action a)` that calls **all** handlers and, after the walk, throws an `AggregateException` with all errors.

**Expected result:**

```text
first
third
errors=1: boom
```

<details><summary>Hint</summary>

`a.GetInvocationList()` returns the individual delegates; call each in its own `try/catch`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: call every handler even if one throws.
static void InvokeAll(Action a)
{
    var errors = new List<Exception>();
    foreach (Action h in a.GetInvocationList())
    {
        try { h(); }
        catch (Exception e) { errors.Add(e); }
    }
    if (errors.Count > 0) throw new AggregateException(errors);
}
```

Verified: the output matches the table. A plain call `a()` would stop at the second handler.

</details>

## 4. 🟡 A closure in a loop

**Task:** in a `for` loop create three functions `() => i`. Show that they return `3,3,3`, and fix it to get `0,1,2`.

**Expected result:**

```text
3,3,3 | 0,1,2
```

<details><summary>Hint</summary>

A closure captures the variable, not the value. Copy `i` into a local variable inside the loop body.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: a closure captures the variable, not its value.
var bad = new List<Func<int>>();
for (int i = 0; i < 3; i++) bad.Add(() => i);

var good = new List<Func<int>>();
for (int i = 0; i < 3; i++) { int copy = i; good.Add(() => copy); }

Console.WriteLine(string.Join(",", bad.Select(f => f())) + " | " + string.Join(",", good.Select(f => f())));
```

Verified: `3,3,3 | 0,1,2`. In `foreach` the variable is new on every iteration, so no copy is needed there.

</details>

## 5. 🔴 A subscription with IDisposable

**Task:** a class `Publisher` with a private event `Message` and a method `IDisposable Subscribe(Action<string> handler)`; the result's `Dispose` unsubscribes the handler. The method `Publish(string)` broadcasts a message. Show that after `Dispose` messages stop arriving.

**Expected result:**

```text
Publish("a"); sub.Dispose(); Publish("b") → received: a, subscribers: 0
```

<details><summary>Hint</summary>

Return a small `IDisposable` whose `Dispose` calls `Message -= handler`. Make `Dispose` idempotent.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: unsubscribing through IDisposable avoids leaks and the "same lambda" trap.
class Publisher
{
    private event Action<string>? Message;
    private int _n;
    public int Count => _n;

    public IDisposable Subscribe(Action<string> handler)
    {
        Message += handler; _n++;
        return new Unsubscriber(() => { Message -= handler; _n--; });
    }

    public void Publish(string m) => Message?.Invoke(m);

    private sealed class Unsubscriber(Action onDispose) : IDisposable
    {
        private Action? _a = onDispose;
        public void Dispose() { _a?.Invoke(); _a = null; }
    }
}
```

Verified: `a` received, subscribers `0`. The handler is kept in a closure, so unsubscribing works even for a lambda.

</details>
