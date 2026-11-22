---
title: "Quiz: Delegates, events, lambdas"
lang: en
track: csharp
kind: module
doc: quiz
slug: 09-delegates-events-lambdas
part: 1
tags: [delegates, lambdas, events, closures, multicast, func-action]
status: draft
updated: 2026-11-22
---

# Quiz: Delegates, events, lambdas

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which standard delegate type fits a method that takes an `int` and returns a `string`?

- A) `Action<int>`
- B) `Func<int, string>`
- C) `Predicate<int>`
- D) `Func<string, int>`

<details><summary>Answer</summary>

**B.** In `Func` the **last** type parameter is the result, the rest are arguments. `Action` returns nothing, `Predicate<T>` returns `bool`.
→ [Basics](basics.en.md#ready-made-types-func-action-predicate)

</details>

## 2. 🟢 What does the code print?

```csharp
static Func<int, int> Compose(Func<int, int> f, Func<int, int> g) => x => g(f(x));
var inc = (int x) => x + 1;
var dbl = (int x) => x * 2;
Console.WriteLine($"{Compose(inc, dbl)(5)} {Compose(dbl, inc)(5)}");
```

<details><summary>Answer</summary>

```text
12 11
```

`Compose(inc, dbl)(5)` = `dbl(inc(5))` = `12`; `Compose(dbl, inc)(5)` = `inc(dbl(5))` = `11`. The lambda `x => g(f(x))` captures `f` and `g` (verified).
→ [Basics](basics.en.md#lambda-expressions)

</details>

## 3. 🟢 What may be done with an `event` from outside the declaring class?

- A) invoke it and subscribe
- B) only subscribe and unsubscribe (`+=`, `-=`)
- C) only invoke it
- D) assign `null`

<details><summary>Answer</summary>

**B.** An event protects the delegate: `c.Evt();` and `c.Evt = null;` outside the class are error `CS0070` (verified). For a public delegate field both operations would be allowed, which is why a field is not used for notifications.
→ [Basics](basics.en.md#events)

</details>

## 4. 🟢 What does a multicast `Func<int>` built from methods returning `1`, `2`, `3` return?

- A) `1`
- B) `6`
- C) `3`
- D) an array `[1, 2, 3]`

<details><summary>Answer</summary>

**C.** All handlers are called in order, but the result is the last one's; the values of the first two are lost (verified). To collect all results use `GetInvocationList()`.
→ [Advanced theory](advanced.en.md#multicast-delegates)

</details>

## 5. 🟡 What does the code print?

```csharp
Action many = () => Console.WriteLine("first");
many += () => throw new InvalidOperationException("boom");
many += () => Console.WriteLine("third");
many();
```

- A) `first`, `third`, then an exception
- B) `first`, then `InvalidOperationException`; `third` does not run
- C) `first`, `third` without an exception
- D) nothing: the exception occurs before the first output

<details><summary>Answer</summary>

**B.** An exception interrupts a multicast call (verified: `1 → throw → 3`, the third did not run). For an independent call walk `GetInvocationList()` and wrap each call in `try`.
→ [Expert theory](expert.en.md#an-exception-interrupts-a-multicast-call)

</details>

## 6. 🟡 Does the unsubscription work?

```csharp
Action a = () => Console.WriteLine("lam");
Action combined = a;
combined -= () => Console.WriteLine("lam");   // the same text
combined();
```

- A) prints `lam` — the handler stayed
- B) prints nothing
- C) an exception
- D) a compile error

<details><summary>Answer</summary>

**A.** Delegates are compared by method and target, and two lambda expressions with the same text are different methods (`(Func<int>)(() => 1) == (Func<int>)(() => 1)` → `False`). To unsubscribe, keep a reference to the original delegate or return an `IDisposable`.
→ [Expert theory](expert.en.md#unsubscribing-with-another-copy-of-a-lambda-does-not-work)

</details>

## 7. 🟡 What does the code print?

```csharp
var bad = new List<Func<int>>();
for (int i = 0; i < 3; i++) bad.Add(() => i);

var good = new List<Func<int>>();
for (int i = 0; i < 3; i++) { int copy = i; good.Add(() => copy); }

Console.WriteLine(string.Join(",", bad.Select(f => f())) + " | " + string.Join(",", good.Select(f => f())));
```

<details><summary>Answer</summary>

```text
3,3,3 | 0,1,2
```

A closure captures the **variable**, not the value. In `for` it is one variable for the whole loop and equals `3` at call time; the copy inside the body is created anew on each iteration (verified).
→ [Advanced theory](advanced.en.md#closures-and-their-cost)

</details>

## 8. 🟡 Select all correct statements

- [ ] A) `Action copy = orig; orig += handler;` — `copy` also gets `handler`
- [ ] B) `+=` on a delegate creates a new delegate
- [ ] C) `n?.Invoke()` is safe for a `null` delegate
- [ ] D) calling `n()` on a `null` delegate throws `NullReferenceException`

<details><summary>Answer</summary>

**B, C, D.** A is wrong: delegates are immutable, `+=` creates a new object, and `copy` keeps pointing at the old one (verified).
→ [Advanced theory](advanced.en.md#multicast-delegates)

</details>

## 9. 🟡 Open question: how can an event cause a memory leak and how do you avoid it?

<details><summary>Key points of a good answer</summary>

- the publisher holds references to subscribers (in the delegate's invocation list): a long-lived publisher keeps short-lived subscribers and everything they captured alive;
- always unsubscribe (`-=`) when the subscriber's life ends — for example in `Dispose`;
- a handy technique is a `Subscribe` method returning an `IDisposable` (unsubscribing in `Dispose`);
- alternatives: weak events (`WeakReference`), a short-lived publisher;
- lambda handlers cannot be unsubscribed with an "identical" lambda — keep a reference to the delegate ([12 · Memory and GC](../12-memory-and-gc/overview.en.md)).

</details>

## 10. 🔴 What does a `static` lambda give and when does it not compile?

```csharp
int x = 5;
Func<int> f = static () => x;
```

<details><summary>Answer</summary>

Error `CS8820`: a `static` lambda cannot capture locals or `this` (verified). In return the compiler guarantees there is no capture: such a lambda allocates no memory (the delegate is cached), whereas a capturing lambda creates a closure object (80 bytes in the measurement), and an instance method group creates a delegate on each conversion (56 bytes; 0 for a static method).
→ [Expert theory](expert.en.md#a-static-lambda-and-capturing)

</details>

## 11. 🔴 What happens if a handler unsubscribes itself during an event call?

```csharp
// a one-shot handler: removes itself on the first call
```

- A) `InvalidOperationException`
- B) it is safe: a copy of the invocation list taken at the start is called; the handler fires once
- C) all other handlers are skipped
- D) an infinite loop

<details><summary>Answer</summary>

**B.** A delegate is immutable: unsubscribing creates a new delegate, while the current call keeps walking the old list (verified: the first broadcast called both, the second only the remaining one).
→ [Expert theory](expert.en.md#unsubscribing-during-a-call-is-safe)

</details>
