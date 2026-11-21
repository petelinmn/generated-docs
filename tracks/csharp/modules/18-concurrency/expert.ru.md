---
title: "Многопоточность и синхронизация: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 18-concurrency
part: 3
tags: [concurrency, memory-model, volatile, monitor, lock-free, compare-and-swap, spinlock, async-local, thread-pool]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-21
---

# Многопоточность и синхронизация: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

## Как устроен lock

Для объекта, который не является `System.Threading.Lock`, оператор `lock (x) { … }` компилируется в `Monitor.Enter`/`Monitor.Exit` внутри `try/finally`. Тот же код вручную:

```csharp
object gate = new();
int shared = 0;
bool lockTaken = false;
try
{
    Monitor.Enter(gate, ref lockTaken);
    shared++;
}
finally
{
    if (lockTaken)
    {
        Monitor.Exit(gate);
    }
}
```

Если компилятор знает, что `x` имеет тип `System.Threading.Lock` (.NET 9+), `lock` превращается в `using (x.EnterScope()) { … }`. `EnterScope()` возвращает `ref struct` с методом `Dispose()`. Свойства обоих механизмов:

- оба **реентерабельны**: поток, уже владеющий блокировкой, может войти в неё повторно — вложенный `lock (newLock) { lock (newLock) { … } }` работает;
- оба привязаны к **потоку-владельцу**: `Monitor.Exit` из потока, который не владеет блокировкой, бросает `SynchronizationLockException` («The calling thread does not hold the lock»);
- `Lock` работает так, только если тип выражения — ровно `Lock`. Если привести его к `object` и заблокировать, используется обычный `Monitor`, а компилятор выдаёт предупреждение CS9216.

## Модель памяти: volatile, Volatile и барьеры

Синхронизация решает две задачи: **атомарность** (никто не видит операцию наполовину) и **видимость/порядок** (изменения одного потока видны другому в правильном порядке). Модель памяти .NET позволяет компилятору и процессору переупорядочивать обычные чтения и записи, если это сохраняет согласованность **в одном потоке**. Для других потоков порядок не гарантирован.

| Инструмент | Гарантия |
|---|---|
| `lock`, `Monitor`, `Lock` | взаимное исключение плюс полная видимость изменений, сделанных под блокировкой |
| `Interlocked.*` | атомарность и **полный барьер** (full fence) |
| `Volatile.Read` | acquire: последующие чтения и записи не выполняются раньше него |
| `Volatile.Write` | release: предыдущие чтения и записи не выполняются позже него |
| поле `volatile` | каждое чтение — `Volatile.Read`, каждая запись — `Volatile.Write` |

Ограничения `volatile`:

- атомарности он не даёт ни для чего, кроме присваивания: `volatile int x; x++` по-прежнему гонка;
- применим только к полям ссылочных типов, указателей, `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `char`, `float`, `bool`, перечислений на их основе, `IntPtr`/`UIntPtr`. `volatile long` и `volatile double` — ошибка CS0677;
- на многопроцессорной системе volatile-чтение не гарантирует, что получено самое последнее записанное значение.

64-битные чтения атомарны в 64-битном процессе. В 32-битном процессе чтение `long` может быть «разорванным» (torn), для него есть `Interlocked.Read`.

**Публикация объекта.** Присваивание ссылки в место, доступное другим потокам, — это release-операция по отношению к полям объекта. Поэтому если поток создал объект и сохранил ссылку в общем поле, другой поток, прочитавший ссылку, увидит полностью сконструированный объект. На этом основано безопасное «ленивое» создание с двойной проверкой ([упражнение 2](exercises.ru.md)): быстрый путь читает поле через `Volatile.Read`, медленный создаёт объект под `lock`.

## Monitor.Wait и Monitor.Pulse

`Monitor` умеет не только исключать, но и ждать условия. `Monitor.Wait(gate)` атомарно **отпускает** блокировку и усыпляет поток, а перед возвратом снова её захватывает. `Pulse` будит одного ожидающего, `PulseAll` — всех. На этом строятся очереди с ожиданием:

```csharp
public void Add(T item)
{
    lock (_gate)
    {
        // Always wait in a loop: after waking up, the condition may already be false again.
        while (_items.Count == capacity)
        {
            Monitor.Wait(_gate); // releases the lock while waiting, re-acquires it before returning
        }
        _items.Enqueue(item);
        Monitor.PulseAll(_gate);
    }
}
```

Условие проверяется **в цикле `while`**, а не в `if`: проснувшийся поток снова конкурирует за блокировку, и к моменту, когда он её получил, другой поток мог уже забрать место или элемент. Буфер ёмкостью 4 передаёт 1000 чисел от производителя потребителю без потерь, сумма — `500500`. В прикладном коде вместо этого используют `Channel<T>`. Код: [Ex09_BlockingBuffer.cs](code/Examples/Ex09_BlockingBuffer.cs).

## Lock-free: CompareExchange

`Interlocked.CompareExchange(ref location, value, comparand)` атомарно записывает `value`, только если в `location` всё ещё лежит `comparand`, и возвращает прежнее значение. Цикл «прочитать → вычислить → попробовать записать → повторить при неудаче» (compare-and-swap, CAS) позволяет строить структуры данных без блокировок:

```csharp
public void Push(T value)
{
    var node = new Node(value, null);
    while (true)
    {
        Node? head = Volatile.Read(ref _head);
        node.Next = head;
        // Publish the node only if nobody changed the head since we read it; otherwise retry.
        if (Interlocked.CompareExchange(ref _head, node, head) == head)
        {
            return;
        }
    }
}
```

10 000 параллельных `Push` дают ровно 10 000 элементов. Lock-free-код не блокирует потоки, но его сложно написать правильно: при большой конкуренции потоки много раз повторяют цикл впустую. Классическая проблема ABA (значение сменилось на другое и вернулось обратно) в таком стеке на .NET не возникает: пока у потока есть ссылка на узел, сборщик мусора не освободит его и не отдаст память новому узлу. Код: [Ex10_LockFreeStack.cs](code/Examples/Ex10_LockFreeStack.cs).

## SpinLock и SpinWait

`SpinLock` не усыпляет поток, а крутится в цикле, пока блокировка не освободится. Это выгодно только для очень коротких участков, и документация советует использовать его, лишь если измерения показали выигрыш. Главная ловушка — `SpinLock` является **структурой**:

```csharp
// Wrong: readonly makes the compiler pass a defensive copy to Enter and to Exit.
#pragma warning disable IDE0044
private readonly SpinLock _spin = new(enableThreadOwnerTracking: true);
#pragma warning restore IDE0044

public void EnterAndExit()
{
    bool taken = false;
    _spin.Enter(ref taken); // locks a temporary copy
    _spin.Exit();           // unlocks another copy, which was never locked
}
```

Вызов бросает `SynchronizationLockException`: `Exit` вызван на копии, которая не была заблокирована. Это та же защитная копия, что и у изменяемой структуры в `in`-параметре (см. [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/expert.ru.md)). `SpinLock` нельзя копировать и нельзя хранить в `readonly`-поле. Кроме того, он **не реентерабелен**: повторный `Enter` тем же потоком при включённом отслеживании владельца бросает `LockRecursionException`. `SpinWait` — помощник для собственных циклов ожидания: он сам решает, когда крутиться, а когда уступить процессор. Код: [Ex11_SpinLockPitfall.cs](code/Examples/Ex11_SpinLockPitfall.cs).

## Данные потока: AsyncLocal, ThreadLocal, ThreadStatic

| Тип | Значение привязано к | После `await` / в новом потоке |
|---|---|---|
| `[ThreadStatic] static` поле | потоку ОС | у каждого потока своё значение |
| `ThreadLocal<T>` | потоку ОС, с фабрикой начального значения | у каждого потока своё значение |
| `AsyncLocal<T>` | логическому потоку выполнения (ExecutionContext) | значение «переезжает» вместе с асинхронным кодом |

```csharp
var asyncLocal = new AsyncLocal<string>();
using var threadLocal = new ThreadLocal<string>();
asyncLocal.Value = "request-42";
threadLocal.Value = "request-42";

(string?, string?) seen = default;
// Thread.Start captures the current ExecutionContext, and AsyncLocal values live in it.
var thread = new Thread(() => seen = (asyncLocal.Value, threadLocal.Value));
thread.Start();
thread.Join();
```

Новый поток видит `("request-42", null)`: значение `AsyncLocal` пришло вместе с контекстом выполнения, а `ThreadLocal` у нового потока пуст. Поэтому данные запроса (идентификатор, пользователь) в асинхронном коде хранят в `AsyncLocal`, а не в `ThreadLocal`: после `await` код может продолжиться на другом потоке. Код: [Ex12_ContextFlow.cs](code/Examples/Ex12_ContextFlow.cs).

## Пул потоков изнутри

- Пул создаёт рабочие потоки по требованию, пока их число не достигнет минимума (`ThreadPool.GetMinThreads`). После этого он может добавить потоки или подождать, пока освободятся занятые. На машине автора минимум равен числу логических процессоров: `12` при `Environment.ProcessorCount = 12`.
- Число потоков пула ограничено. Если много потоков пула заблокировано (`.Result`, `.Wait()`, `Thread.Sleep`, синхронный ввод-вывод), новые задачи не могут начаться — это **голодание пула** (thread pool starvation). Лечение — асинхронный код без блокирующих ожиданий.
- `TaskCreationOptions.LongRunning` — подсказка планировщику. Стандартный планировщик выполняет такую задачу не на потоке пула (`IsThreadPoolThread = False`), поэтому долгая блокирующая работа не отнимает у пула поток.

## Справочник

### Примитивы синхронизации

| Примитив | Вид | Реентерабельность | Владелец | Async | Между процессами |
|---|---|---|---|---|---|
| `lock` / `Monitor` | облегчённый | да | поток | нет | нет |
| `System.Threading.Lock` (.NET 9+) | облегчённый | да | поток | нет | нет |
| `SpinLock` | облегчённый, структура | нет (`LockRecursionException`) | поток (если включено отслеживание) | нет | нет |
| `Interlocked` | атомарные операции | — | — | — | нет |
| `SemaphoreSlim` | облегчённый | нет: повторный `Wait` того же потока ждёт как чужой | нет | `WaitAsync` | нет |
| `Semaphore` | `WaitHandle` | нет | нет | нет | именованный — только Windows |
| `Mutex` | `WaitHandle` | да | поток: `ReleaseMutex` из чужого потока бросает `ApplicationException` | нет | да; в Unix по умолчанию в пределах пользователя |
| `ReaderWriterLockSlim` | облегчённый | по умолчанию нет (`NoRecursion`) | поток | нет | нет |
| `ManualResetEventSlim` | облегчённый | — | — | нет | нет |
| `AutoResetEvent`, `ManualResetEvent`, `EventWaitHandle` | `WaitHandle` | — | — | нет | именованный `EventWaitHandle` — только Windows |
| `CountdownEvent`, `Barrier` | облегчённые | — | — | нет | нет |
| `Channel<T>` | асинхронная очередь | — | — | да | нет |

Облегчённые типы не используют объекты ядра ОС и обычно быстрее, но работают только внутри процесса.

### Ожидание с таймаутом и отменой

| Примитив | Ожидание с таймаутом | Отмена |
|---|---|---|
| `Monitor` | `Monitor.TryEnter(obj, timeout)` | нет |
| `SemaphoreSlim` | `Wait(timeout)`, `WaitAsync(timeout)` | `CancellationToken` |
| `ManualResetEventSlim`, `CountdownEvent`, `Barrier` | `Wait(timeout)`, `SignalAndWait(timeout)` | `CancellationToken` |
| `WaitHandle` (`Mutex`, `Semaphore`, события) | `WaitOne(timeout)`, `WaitHandle.WaitAll/WaitAny` | нет |
| `Channel<T>` | — | `CancellationToken` в `ReadAsync`/`WriteAsync` |

### Потокобезопасные коллекции

| Коллекция | Порядок | Особенность |
|---|---|---|
| `ConcurrentDictionary<TKey, TValue>` | нет | атомарные `TryAdd`, `TryRemove`, `AddOrUpdate`, `GetOrAdd`; делегаты выполняются вне блокировки |
| `ConcurrentQueue<T>` | FIFO | `TryDequeue` |
| `ConcurrentStack<T>` | LIFO | `TryPop`, `PushRange` |
| `ConcurrentBag<T>` | нет | быстрее всего, когда поток сам потребляет то, что добавил |
| `BlockingCollection<T>` | зависит от хранилища | блокирующие `Add`/`Take`, ограничение ёмкости; в новом коде — `Channel<T>` |
| `ImmutableDictionary`, `ImmutableList` | — | неизменяемые: «изменение» создаёт новую коллекцию, читать можно без блокировок |

## Подвохи

### Lock, приведённый к object

```csharp
Lock l = new();
object o = l;
lock (o) { }   // warning CS9216: Monitor-based locking is used
```

Новый быстрый механизм работает, только если статический тип выражения — `Lock`. Через `object` получается обычный `Monitor`.

### await внутри lock

`lock (gate) { await Task.Delay(1); }` — ошибка компиляции CS1996. После `await` код может продолжиться в другом потоке, а `Monitor` привязан к потоку-владельцу. Решение — `SemaphoreSlim.WaitAsync`.

### lock по значимому типу

`int v = 1; lock (v) { }` — ошибка CS0185: блокировать можно только ссылочный тип. Упаковка при каждом вызове создавала бы новый объект (см. [02 · Типы и переменные](../02-types-and-variables/advanced.ru.md)), и блокировка ничего бы не защищала.

### volatile не делает ++ атомарным

`volatile` гарантирует порядок и видимость, но `x++` всё равно состоит из чтения и записи. Для счётчиков — `Interlocked.Increment`. `volatile long` вообще не компилируется (CS0677).

### Interlocked и свойство

`Interlocked.Increment(ref c.P)` для свойства — ошибка CS0206: `ref` нужен на переменную или поле, а свойство — это методы. Атомарно изменяемое значение хранят в поле.

### SemaphoreSlim не реентерабелен

```csharp
var sem = new SemaphoreSlim(1, 1);
sem.Wait();
sem.Wait(100);   // False: the same thread waits like any other
```

У семафора нет владельца, только счётчик. Рекурсивный вызов метода, который снова ждёт тот же `SemaphoreSlim(1, 1)`, без таймаута зависнет навсегда.

### ReaderWriterLockSlim и рекурсия

При политике по умолчанию `NoRecursion` повторный `EnterWriteLock` тем же потоком бросает `LockRecursionException` («Recursive write lock acquisitions not allowed in this mode»), как и попытка взять запись при удерживаемом чтении.

### Mutex помнит владельца

`Mutex` можно захватить повторно в том же потоке (`WaitOne` дважды), но освободить — только в нём: `ReleaseMutex` из другого потока бросает `ApplicationException` («Object synchronization method was called from an unsynchronized block of code»). Если поток завершился, не освободив мьютекс, следующий захвативший получит `AbandonedMutexException`. Из-за привязки к потоку `Mutex` нельзя использовать вокруг `await`.

### Перечисление словаря при изменении

Обычный `Dictionary` при добавлении элемента во время `foreach` бросает `InvalidOperationException` («Collection was modified»). `ConcurrentDictionary` в той же ситуации исключения не бросает, но перечисление может как увидеть новые элементы, так и не увидеть их.

### Random из нескольких потоков

`Random` не потокобезопасен. Документация предупреждает: при обращении без синхронизации методы начинают возвращать 0. Для общего генератора есть `Random.Shared`.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `lock` / `Lock` | просто, понятно, реентерабельно | блокирует потоки; нельзя использовать с `await` |
| `Interlocked` | без блокировок, быстро | только одна переменная |
| Lock-free структуры | нет блокировок и взаимоблокировок | сложно написать и проверить; ретраи при конкуренции |
| Concurrent-коллекции | готово и проверено | атомарны только отдельные операции, не их последовательность |
| Неизменяемые данные | читать можно без синхронизации | каждое изменение создаёт новый объект |
| Передача сообщений (`Channel<T>`) | нет общего состояния | другой стиль архитектуры; задержки на очередь |

Иерархия выбора: сначала убери общее изменяемое состояние; если не получилось — `Interlocked` для одной переменной, `lock` для нескольких, специализированные примитивы — только под конкретную задачу.

## Вопросы с собеседований

**1. Что такое гонка данных и почему `count++` не потокобезопасен?**

<details><summary>Ответ</summary>

Гонка — ситуация, когда результат зависит от порядка выполнения потоков. `count++` состоит из чтения, сложения и записи. Два потока могут прочитать одно значение, и одно увеличение потеряется: 4 потока по миллиону итераций дали 1 966 527 вместо 4 000 000.

</details>

**2. Во что компилируется `lock`?**

<details><summary>Ответ</summary>

Для обычного объекта — в `Monitor.Enter(obj, ref taken)` внутри `try` и `Monitor.Exit` в `finally`. Для выражения типа `System.Threading.Lock` (.NET 9+) — в `using (x.EnterScope())`.

</details>

**3. Чем `System.Threading.Lock` лучше `lock` по `object`?**

<details><summary>Ответ</summary>

Это отдельный тип, созданный только для взаимного исключения, и документация рекомендует его для лучшей производительности. Подвох: если привести `Lock` к `object`, используется обычный `Monitor` и компилятор предупреждает (CS9216).

</details>

**4. Почему нельзя `await` внутри `lock`? Что использовать вместо?**

<details><summary>Ответ</summary>

Это ошибка компиляции CS1996. `Monitor` привязан к потоку, а после `await` выполнение может продолжиться на другом потоке. Вместо `lock` — `SemaphoreSlim(1, 1)` с `await WaitAsync()` и `Release()` в `finally`.

</details>

**5. Чем `SemaphoreSlim` отличается от `Semaphore`, а `Mutex` от `lock`?**

<details><summary>Ответ</summary>

`SemaphoreSlim` — облегчённый, внутри процесса, умеет `WaitAsync`. `Semaphore` — объект ядра, может быть именованным для синхронизации процессов (только в Windows). `Mutex` тоже объект ядра: работает между процессами, но медленнее `lock` и, как и `lock`, привязан к потоку-владельцу.

</details>

**6. Что такое взаимоблокировка и как её избежать?**

<details><summary>Ответ</summary>

Потоки ждут друг друга по кругу. Способы: единый порядок захвата замков, держать один замок за раз, таймауты (`TryEnter`, `Wait(timeout)`), не вызывать чужой код под блокировкой.

</details>

**7. Что гарантирует `volatile` и чего не гарантирует?**

<details><summary>Ответ</summary>

Гарантирует порядок: чтение — acquire, запись — release. Не гарантирует атомарность составных операций (`x++`) и не гарантирует, что чтение увидит самое последнее значение на многопроцессорной системе. Неприменим к `long` и `double`.

</details>

**8. Зачем нужна двойная проверка в ленивой инициализации и почему там `Volatile.Read`?**

<details><summary>Ответ</summary>

Первая проверка без блокировки делает быстрым частый путь, вторая под `lock` не даёт двум потокам создать объект дважды. `Volatile.Read`/`Volatile.Write` упорядочивают чтение и публикацию ссылки. В прикладном коде проще взять `Lazy<T>` или `LazyInitializer`.

</details>

**9. 🪤 Сколько раз выполнится фабрика в `ConcurrentDictionary.GetOrAdd(key, factory)` при одновременном вызове из 8 потоков?**

<details><summary>Ответ</summary>

От 1 до 8: фабрика выполняется вне блокировки словаря. Проверено — 8 раз, когда все потоки одновременно оказались внутри фабрики. В словарь попадёт одно значение. Если фабрика дорогая — `GetOrAdd(key, _ => new Lazy<T>(factory)).Value`.

</details>

**10. 🪤 Почему `SpinLock` в `readonly`-поле бросает `SynchronizationLockException`?**

<details><summary>Ответ</summary>

`SpinLock` — структура. Для `readonly`-поля компилятор передаёт в каждый вызов защитную копию: `Enter` блокирует одну копию, а `Exit` вызывается на другой, незаблокированной.

</details>

**11. 🪤 Поток держит `SemaphoreSlim(1, 1)` и снова вызывает `Wait()`. Что будет?**

<details><summary>Ответ</summary>

Поток зависнет: у семафора нет владельца, и повторное ожидание того же потока ничем не отличается от чужого. `lock`, `Lock` и `Mutex` в этой ситуации реентерабельны.

</details>

**12. 🪤 Можно ли освободить `Mutex` из другого потока?**

<details><summary>Ответ</summary>

Нет: `ReleaseMutex` из потока, который не владеет мьютексом, бросает `ApplicationException`. Поэтому `Mutex` не используют вокруг `await`.

</details>

**13. Что такое голодание пула потоков?**

<details><summary>Ответ</summary>

Когда потоки пула заблокированы ожиданием (`.Result`, `.Wait()`, `Thread.Sleep`), а пул ограничен, новые задачи не могут начаться и приложение «замирает». Лечение — асинхронный код без блокирующих ожиданий; для долгой блокирующей работы — `TaskCreationOptions.LongRunning`.

</details>

**14. Чем `AsyncLocal<T>` отличается от `ThreadLocal<T>`?**

<details><summary>Ответ</summary>

`ThreadLocal` хранит значение для потока ОС. `AsyncLocal` хранит его в контексте выполнения, который переходит в продолжения после `await` и в новые потоки. Для данных запроса в асинхронном коде нужен `AsyncLocal`.

</details>

**15. Почему условие для `Monitor.Wait` проверяют в цикле `while`?**

<details><summary>Ответ</summary>

После пробуждения поток снова конкурирует за блокировку. Пока он её получал, другой поток мог изменить состояние, и условие опять ложно. `while` перепроверяет его после каждого пробуждения.

</details>

## Шпаргалка

| Задача | Как |
|---|---|
| Защитить данные | `private readonly Lock _gate = new();` + `lock (_gate) { … }` |
| Счётчик | `Interlocked.Increment(ref _count)` |
| Блокировка с `await` | `await _sem.WaitAsync(); try { … } finally { _sem.Release(); }` |
| Ограничить параллелизм | `new SemaphoreSlim(N, N)` или `Parallel.ForEachAsync` с `MaxDegreeOfParallelism` |
| Много читателей | `ReaderWriterLockSlim` |
| Очередь производитель–потребитель | `Channel.CreateBounded<T>(capacity)` |
| Дорогая фабрика в словаре | `ConcurrentDictionary<K, Lazy<V>>` |
| Ждать с таймаутом | `Monitor.TryEnter(obj, timeout)`, `Wait(timeout)` |
| Данные запроса в async-коде | `AsyncLocal<T>` |
| Случайные числа из многих потоков | `Random.Shared` |

## Связанные материалы

- Раньше: [продвинутая теория](advanced.ru.md)
- Защитные копии структур: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/expert.ru.md)
- Упаковка значимых типов: [02 · Типы и переменные](../02-types-and-variables/advanced.ru.md)
- Сборщик мусора и время жизни объектов: [12 · Память и GC](../12-memory-and-gc/expert.ru.md)
- Как EF Core обнаруживает параллельное использование контекста: [15 · EF Core](../15-ef-core/expert.ru.md#детектор-параллельного-использования)
- То же в других треках: [Python](../../../python/overview.ru.md) — GIL и модели параллелизма

## Источники

- [Оператор lock](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock)
- [`System.Threading.Lock`](https://learn.microsoft.com/dotnet/api/system.threading.lock)
- [Ключевое слово volatile](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/volatile)
- [Модель памяти .NET](https://github.com/dotnet/runtime/blob/main/docs/design/specs/Memory-model.md)
- [`Interlocked.Read`](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.read)
- [`SpinLock`](https://learn.microsoft.com/dotnet/api/system.threading.spinlock)
- [`Mutex`](https://learn.microsoft.com/dotnet/api/system.threading.mutex)
- [`AsyncLocal<T>`](https://learn.microsoft.com/dotnet/api/system.threading.asynclocal-1)
- [Обзор примитивов синхронизации](https://learn.microsoft.com/dotnet/standard/threading/overview-of-synchronization-primitives)
- [Управляемый пул потоков](https://learn.microsoft.com/dotnet/standard/threading/the-managed-thread-pool)
