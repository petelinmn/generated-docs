---
title: "Упражнения: Делегаты, события, лямбды"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 09-delegates-events-lambdas
part: 1
tags: [delegates, lambdas, events, closures, multicast, idisposable]
status: draft
updated: 2026-11-22
---

# Упражнения: Делегаты, события, лямбды

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Композиция функций

**Задача:** напиши `Compose(Func<int,int> f, Func<int,int> g)`, который возвращает функцию «сначала `f`, потом `g`». Примени к `inc = x + 1` и `dbl = x * 2`.

**Ожидаемый результат:**

```text
Compose(inc, dbl)(5) → 12;  Compose(dbl, inc)(5) → 11
[1, 2, 3].Select(Compose(inc, dbl)) → 4,6,8
```

<details><summary>Подсказка</summary>

Метод возвращает лямбду, которая захватывает `f` и `g`: `x => g(f(x))`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: a function that returns a function (the lambda captures f and g).
static Func<int, int> Compose(Func<int, int> f, Func<int, int> g) => x => g(f(x));

var inc = (int x) => x + 1;
var dbl = (int x) => x * 2;
Console.WriteLine($"{Compose(inc, dbl)(5)} {Compose(dbl, inc)(5)}");               // 12 11
Console.WriteLine(string.Join(",", new[] { 1, 2, 3 }.Select(Compose(inc, dbl))));  // 4,6,8
```

Проверено: `12 11` и `4,6,8`. Порядок аргументов важен — композиция не коммутативна.

</details>

## 2. 🟢 Событие кнопки

**Задача:** класс `Button` с событием `Clicked` стандартного вида `EventHandler<ClickArgs>` (`ClickArgs` содержит `Label`) и методом `Click(string label)`. Подпишись лямбдой, нажми дважды и покажи число нажатий и последнюю метку.

**Ожидаемый результат:**

```text
Click("OK"), Click("Cancel") → 2 Cancel
```

<details><summary>Подсказка</summary>

Вызывай событие через `Clicked?.Invoke(this, new ClickArgs(label))`: подписчиков может не быть.

</details>

<details><summary>Решение</summary>

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

Проверено: `2 Cancel`. Снаружи класса событие нельзя вызвать — только подписаться.

</details>

## 3. 🟡 Вызов всех обработчиков

**Задача:** многоадресный `Action` состоит из трёх обработчиков, средний бросает исключение. Напиши `InvokeAll(Action a)`, который вызывает **все** обработчики и после обхода бросает `AggregateException` со всеми ошибками.

**Ожидаемый результат:**

```text
first
third
errors=1: boom
```

<details><summary>Подсказка</summary>

`a.GetInvocationList()` возвращает отдельные делегаты; каждый вызывай в своём `try/catch`.

</details>

<details><summary>Решение</summary>

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

Проверено: вывод совпадает с таблицей. Обычный вызов `a()` оборвался бы на втором обработчике.

</details>

## 4. 🟡 Замыкание в цикле

**Задача:** в цикле `for` создай три функции `() => i`. Покажи, что они возвращают `3,3,3`, и исправь, чтобы получилось `0,1,2`.

**Ожидаемый результат:**

```text
3,3,3 | 0,1,2
```

<details><summary>Подсказка</summary>

Замыкание захватывает переменную, а не значение. Скопируй `i` в локальную переменную внутри тела цикла.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: a closure captures the variable, not its value.
var bad = new List<Func<int>>();
for (int i = 0; i < 3; i++) bad.Add(() => i);

var good = new List<Func<int>>();
for (int i = 0; i < 3; i++) { int copy = i; good.Add(() => copy); }

Console.WriteLine(string.Join(",", bad.Select(f => f())) + " | " + string.Join(",", good.Select(f => f())));
```

Проверено: `3,3,3 | 0,1,2`. В `foreach` переменная новая на каждой итерации, поэтому там копия не нужна.

</details>

## 5. 🔴 Подписка с IDisposable

**Задача:** класс `Publisher` с закрытым событием `Message` и методом `IDisposable Subscribe(Action<string> handler)`; `Dispose` результата отписывает обработчик. Метод `Publish(string)` рассылает сообщение. Покажи, что после `Dispose` сообщения не приходят.

**Ожидаемый результат:**

```text
Publish("a"); sub.Dispose(); Publish("b") → получено: a, подписчиков: 0
```

<details><summary>Подсказка</summary>

Возвращай небольшой `IDisposable`, который в `Dispose` вызывает `Message -= handler`. Сделай `Dispose` идемпотентным.

</details>

<details><summary>Решение</summary>

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

Проверено: получено `a`, подписчиков `0`. Обработчик хранится в замыкании, поэтому отписка работает даже для лямбды.

</details>
