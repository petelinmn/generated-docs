---
title: "Упражнения: Многопоточность и синхронизация"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 18-concurrency
part: 3
tags: [lock, double-checked-locking, semaphore, compare-and-swap]
status: draft
updated: 2026-11-21
---

# Упражнения: Многопоточность и синхронизация

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами, запускаемые версии — в [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), тесты к ним — в [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Потокобезопасная статистика

**Задача:** напиши класс `SafeStatistics` с методами `Add(long value)` и `Snapshot()`, который возвращает `(Count, Sum, Max)`. Методы вызываются из многих потоков одновременно, а снимок должен быть согласованным: все три значения — на один момент.

**Ожидаемый результат:**

```text
Parallel.For(1, 10_001, i => stats.Add(i));
stats.Snapshot() → (10000, 50005000, 10000)
```

<details><summary>Подсказка</summary>

`Interlocked` для каждого поля по отдельности не подойдёт: между обновлениями другой поток увидит несогласованный набор. Нужен один замок на всё состояние.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: several fields must change together, so one lock protects all of them.
public sealed class SafeStatistics
{
    private readonly Lock _gate = new();
    private long _count;
    private long _sum;
    private long _max = long.MinValue;

    public void Add(long value)
    {
        lock (_gate)
        {
            _count++;
            _sum += value;
            if (value > _max)
            {
                _max = value;
            }
        }
    }

    // A consistent snapshot: all three values are read under the same lock.
    public (long Count, long Sum, long Max) Snapshot()
    {
        lock (_gate)
        {
            return (_count, _sum, _max);
        }
    }
}
```

Класс `SafeStatistics` в [Solutions.cs](code/Solutions/Solutions.cs). Чтение снимка тоже идёт под замком — иначе можно получить `Count` до добавления, а `Sum` после.

</details>

## 2. 🟡 Ленивая инициализация с двойной проверкой

**Задача:** напиши `OnceInitializer<T>` со свойством `Value`, которое при первом обращении вызывает фабрику, а дальше возвращает тот же объект. Фабрика должна выполниться ровно один раз, даже если 8 потоков обращаются к `Value` одновременно. Частый путь (значение уже создано) не должен брать блокировку.

**Ожидаемый результат:**

```text
8 потоков одновременно читают Value → фабрика вызвана 1 раз, все получили один и тот же объект
```

<details><summary>Подсказка</summary>

Проверка без блокировки → `lock` → повторная проверка под блокировкой → создание. Чтение и публикацию поля оформи через `Volatile.Read` и `Volatile.Write`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: thread-safe lazy initialization with double-checked locking.
public sealed class OnceInitializer<T>(Func<T> factory)
    where T : class
{
    private readonly Lock _gate = new();
    private T? _value;

    public T Value
    {
        get
        {
            // Fast path without a lock; Volatile.Read pairs with Volatile.Write below.
            T? value = Volatile.Read(ref _value);
            if (value is not null)
            {
                return value;
            }
            lock (_gate)
            {
                value = _value;
                if (value is null)
                {
                    value = factory();
                    Volatile.Write(ref _value, value);
                }
                return value;
            }
        }
    }
}
```

Класс `OnceInitializer<T>` в [Solutions.cs](code/Solutions/Solutions.cs). Вторая проверка под блокировкой обязательна: пока поток ждал замок, другой поток мог уже создать объект. В реальном коде то же самое даёт `Lazy<T>`.

</details>

## 3. 🟡 Блокировка по ключу

**Задача:** напиши `KeyedLock` с методом `Task<T> RunAsync<T>(string key, Func<Task<T>> action)`. Действия с одинаковым ключом выполняются строго по одному, с разными — не мешают друг другу. Внутри `action` есть `await`.

**Ожидаемый результат:**

```text
20 вызовов RunAsync("same", …) → одновременно выполняется не больше 1
```

<details><summary>Подсказка</summary>

`lock` не подойдёт из-за `await`. Храни по одному `SemaphoreSlim(1, 1)` на ключ в `ConcurrentDictionary`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: one async-friendly lock per key; different keys do not block each other.
public sealed class KeyedLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<T> RunAsync<T>(string key, Func<Task<T>> action)
    {
        SemaphoreSlim gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            gate.Release();
        }
    }
}
```

Класс `KeyedLock` в [Solutions.cs](code/Solutions/Solutions.cs). `GetOrAdd` может создать лишний `SemaphoreSlim`, но в словарь попадёт и использоваться будет только один. Семафоры здесь не удаляются из словаря — для неограниченного множества ключей это утечка памяти, и в реальном коде нужна очистка.

</details>

## 4. 🔴 Атомарный максимум

**Задача:** напиши `AtomicMath.InterlockedMax(ref int location, int value)`. Метод атомарно записывает `value` в `location`, если оно больше текущего значения, и возвращает итоговое значение. Без `lock`.

**Ожидаемый результат:**

```text
Parallel.For(-5_000, 5_001, i => AtomicMath.InterlockedMax(ref max, i)) → max == 5000
InterlockedMax(ref location /* 10 */, 3)  → 10
InterlockedMax(ref location /* 10 */, 20) → 20
```

<details><summary>Подсказка</summary>

Цикл compare-and-swap: прочитать текущее значение; если новое не больше — выйти; иначе `CompareExchange`. Если он вернул не то, что ты прочитал, значит, кто-то успел изменить значение — повтори с новым.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: a lock-free "store the maximum" built on a compare-and-swap loop.
public static class AtomicMath
{
    public static int InterlockedMax(ref int location, int value)
    {
        int current = Volatile.Read(ref location);
        while (value > current)
        {
            int seen = Interlocked.CompareExchange(ref location, value, current);
            if (seen == current)
            {
                return value; // our value was stored
            }
            current = seen; // someone else changed it: re-check against the new value
        }
        return current;
    }
}
```

Класс `AtomicMath` в [Solutions.cs](code/Solutions/Solutions.cs). Тот же приём используется в [Ex04_Throttling.cs](code/Examples/Ex04_Throttling.cs) для подсчёта пиковой нагрузки и в lock-free-стеке из [экспертной теории](expert.ru.md).

</details>
