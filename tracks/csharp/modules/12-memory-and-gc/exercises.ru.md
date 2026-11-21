---
title: "Упражнения: Память и GC"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 12-memory-and-gc
part: 2
tags: [idisposable, dispose-pattern, memory-leak, weak-reference, weak-event]
status: draft
updated: 2026-11-21
---

# Упражнения: Память и GC

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами, запускаемые версии — в [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), тесты к ним — в [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Временный файл

**Задача:** напиши класс `TempFile : IDisposable`. Конструктор создаёт временный файл с переданным текстом, свойство `Path` возвращает путь, метод `Read()` читает содержимое. `Dispose` удаляет файл. Повторный `Dispose` ничего не делает, а `Read()` после `Dispose` бросает `ObjectDisposedException`.

**Ожидаемый результат:**

```text
var temp = new TempFile("hello");
temp.Read()                → "hello"
temp.Dispose(); temp.Dispose();
File.Exists(temp.Path)     → false
temp.Read()                → ObjectDisposedException
```

<details><summary>Подсказка</summary>

`System.IO.Path.GetTempFileName()` создаёт пустой временный файл и возвращает путь. Флаг `_disposed` делает `Dispose` идемпотентным, а `ObjectDisposedException.ThrowIf(_disposed, this)` — короткая проверка в начале методов.

</details>

<details><summary>Решение</summary>

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

Класс `sealed` и держит только управляемые ресурсы, поэтому шаблон `Dispose(bool)` и финализатор не нужны. `File.Delete` не бросает исключение, если файла нет, — это ещё одна страховка идемпотентности.

</details>

## 2. 🟡 Контейнер освобождаемых объектов

**Задача:** напиши `CompositeDisposable : IDisposable` с методом `Add(IDisposable item)`. `Dispose` освобождает всё добавленное **в обратном порядке**. Если какой-то `Dispose` бросил исключение, остальные всё равно освобождаются, а в конце бросается `AggregateException` со всеми ошибками. Повторный `Dispose` ничего не делает. Объект, добавленный после `Dispose`, освобождается сразу.

**Ожидаемый результат:**

```text
Add(a); Add(b — бросает в Dispose); Add(c)
Dispose() → порядок: c, b, a; AggregateException с одной ошибкой "b"
после Dispose: Add(late) → late освобождён сразу
```

<details><summary>Подсказка</summary>

Обходи список с конца, каждый `Dispose` оберни в `try/catch` и копи исключения. Флаг `_disposed` выставляй до обхода: тогда повторный вызов из какого-нибудь `Dispose` внутри не начнёт обход заново.

</details>

<details><summary>Решение</summary>

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

Обратный порядок повторяет поведение нескольких `using var`: созданное позже освобождается раньше. Здесь `Dispose` бросает исключение сознательно — после того как освобождено всё, иначе ошибка одного объекта оставила бы остальные неосвобождёнными.

</details>

## 3. 🟡 Подписка, которую можно освободить

**Задача:** напиши `MessageHub` с методами `Subscribe(Action<string> handler)` и `Publish(string message)`. `Subscribe` возвращает `IDisposable`: его `Dispose` отписывает обработчик. Повторный `Dispose` безопасен. Отписанный подписчик должен собираться GC.

**Ожидаемый результат:**

```text
var token = hub.Subscribe(listener.On);
hub.Publish("one"); token.Dispose(); token.Dispose(); hub.Publish("two");
listener.Received → ["one"], hub.Count → 0
подписчик, отписанный через using, после GC.Collect() → собран
```

<details><summary>Подсказка</summary>

Верни из `Subscribe` объект вложенного класса, который хранит хаб и обработчик и в `Dispose` удаляет обработчик из списка. После отписки обнули ссылку на хаб: токен не будет удерживать хаб, а повторный вызов станет пустым.

</details>

<details><summary>Решение</summary>

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

`Publish` обходит копию списка: обработчик может отписаться прямо во время рассылки, и обход по исходному списку бросил бы `InvalidOperationException`. Так же устроены подписки в Rx (`IObservable<T>.Subscribe` возвращает `IDisposable`).

</details>

## 4. 🔴 Слабое событие

**Задача:** напиши `WeakEvent<TArgs>` с методами `Subscribe(Action<TArgs> handler)` и `Raise(TArgs args)` и свойством `Count`. Событие не должно удерживать подписчиков: если подписчик больше нигде не используется, GC его собирает, а `Raise` удаляет его запись. Живые подписчики получают события в порядке подписки.

**Ожидаемый результат:**

```text
живой подписчик: Raise(1); Raise(2) → получил [1, 2]
забытый подписчик: GC.Collect() → собран; Raise(1) → Count == 0
```

<details><summary>Подсказка</summary>

Нельзя хранить сам делегат: он ссылается на объект-подписчик (`handler.Target`) сильной ссылкой. Храни `WeakReference<object>` на `Target` и `MethodInfo` (`handler.Method`), а вызывай через `method.Invoke(target, [args])`. У статического метода `Target == null` — такой делегат можно хранить как есть.

</details>

<details><summary>Решение</summary>

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

Ловушка слабых событий — лямбды с замыканием. У лямбды, которая захватывает локальные переменные, `Target` — объект замыкания, на который ссылается только сам делегат. Слабое событие его не удержит, и подписка «умрёт» при ближайшей сборке, хотя подписчик жив. Проверено: подписка `v => sink.Add(v)` получила только первое событие, а после `GC.Collect()` исчезла (`Count == 0`), хотя список `sink` продолжал использоваться. Поэтому подписываются методом экземпляра (`listener.On`), а не лямбдой. Вызов через `MethodInfo.Invoke` медленнее прямого вызова делегата — для частых событий делегат строят один раз, например через `CreateDelegate`.

</details>
