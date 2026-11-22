---
title: "Async/await: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 10-async-await
part: 2
tags: [async-await, state-machine, awaitable, execution-context, async-local, valuetask, task-scheduler, thread-pool-starvation, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Async/await: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая](basics.ru.md) и [продвинутая](advanced.ru.md) теория. **Время:** ~2 ч.

## Как компилятор превращает async-метод

Метод с `async` компилируется в **конечный автомат** (state machine) — скрытый тип, реализующий `IAsyncStateMachine`. Локальные переменные и параметры, живущие через `await`, становятся его полями; метод-обёртка создаёт автомат, вызывает `builder.Start(ref machine)` и возвращает `builder.Task`. Проверено рефлексией на методе `RunAsync(int x)` с двумя `await`:

```csharp
class Demo
{
    public async Task<int> RunAsync(int x) { await Task.Delay(1); var y = x + 1; await Task.Delay(1); return y; }
}

foreach (var t in typeof(Demo).GetNestedTypes(BindingFlags.NonPublic))
    Console.WriteLine($"{t.Name}  IsValueType={t.IsValueType}  interfaces={string.Join(",", t.GetInterfaces().Select(i => i.Name))}");
```

```text
Debug:   <RunAsync>d__0  IsValueType=False  interfaces=IAsyncStateMachine
Release: <RunAsync>d__0  IsValueType=True   interfaces=IAsyncStateMachine
```

Поля автомата: `<>1__state` (номер точки продолжения), `<>t__builder`, параметры и переменные, пережившие `await` (в Release локальная `y` хранится как `<y>5__2`), и поля `<>u__N` для awaiter-ов. В Release автомат — **структура**: пока метод завершается синхронно, он живёт в стеке. При первом реальном приостановлении автомат упаковывается в кучу (бокс) — одно выделение на всё время жизни вызова; в Debug это класс, и выделение происходит всегда.

Каждый `await expr` разворачивается примерно так:

1. `awaiter = expr.GetAwaiter()`;
2. если `awaiter.IsCompleted` — результат берётся сразу, без приостановки (быстрый путь);
3. иначе в `<>1__state` записывается номер, awaiter сохраняется в поле, вызывается `builder.AwaitUnsafeOnCompleted(ref awaiter, ref machine)` и метод возвращается;
4. при завершении awaiter вызывает `MoveNext()`, автомат переходит к нужной точке, и вызывается `awaiter.GetResult()` (он же бросает исключение).

### Awaitable-шаблон

`await` работает не только с `Task`: достаточно типа с методом `GetAwaiter()`, возвращающего тип с `IsCompleted`, `OnCompleted` (через `INotifyCompletion`) и `GetResult()`. Проверено на минимальной реализации:

```csharp
struct MyAwaitable { public MyAwaiter GetAwaiter() => new(); }

struct MyAwaiter : INotifyCompletion
{
    public bool IsCompleted => false;
    public void OnCompleted(Action continuation) { Console.WriteLine("OnCompleted called"); continuation(); }
    public void GetResult() { Console.WriteLine("GetResult called"); }
}

await new MyAwaitable();
// OnCompleted called
// GetResult called
```

`GetAwaiter` может быть и методом расширения — так `await` работает для `TimeSpan`, `Process` и подобного в сторонних библиотеках.

### Builder-ы и возвращаемые типы

Тип результата `async`-метода определяет builder: `Task`/`Task<T>` — `AsyncTaskMethodBuilder`, `ValueTask`/`ValueTask<T>` — `AsyncValueTaskMethodBuilder`, `void` — `AsyncVoidMethodBuilder`. С C# 7 допустим **любой** «task-подобный» тип, помеченный `[AsyncMethodBuilder(typeof(...))]`. Для `ValueTask` можно включить пул автоматов атрибутом `[AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]` на методе (проверку выигрыша делают замером).

## Задачи: состояния и планирование

| Свойство `Status` | Когда |
|---|---|
| `Created` | задача создана конструктором и не запущена (редко) |
| `WaitingForActivation` | «promise»-задача — результат async-метода или `TaskCompletionSource`, ждёт завершения |
| `WaitingToRun` / `Running` | запланирована / выполняется делегат |
| `WaitingForChildrenToComplete` | ждёт вложенных задач, созданных с `AttachedToParent` |
| `RanToCompletion` | успешно |
| `Canceled` | отменена (брошено `OperationCanceledException` с токеном задачи) |
| `Faulted` | завершилась исключением |

Задачи двух видов: **делегатные** (`Task.Run`, `Task.Factory.StartNew`) — выполняют код на планировщике, и **promise-задачи** (результат `async`-метода, `TaskCompletionSource`, `Task.Delay`) — потока не занимают: они «завершаются» снаружи.

### `Task.Run` и `Task.Factory.StartNew`

`Task.Run(async () => ...)` распаковывает вложенную задачу, а `StartNew` — нет и возвращает `Task<Task>`:

```csharp
Task<Task> tt = Task.Factory.StartNew(async () => { await Task.Delay(50); });
Console.WriteLine(tt.IsCompleted);   // False now, but it completes as soon as the lambda reaches its first await
await tt.Unwrap();                   // must unwrap to wait for the inner work

var viaRun = Task.Run(async () => { await Task.Delay(50); });   // already unwrapped
await viaRun;
```

Внешняя задача `StartNew` завершается, когда лямбда дошла до первого `await`, — не когда она закончилась. Кроме того, `StartNew` использует `TaskScheduler.Current`, а не пул, поэтому внутри кастомного планировщика поведение меняется. Используйте `Task.Run`.

### Контекст выполнения: `ExecutionContext` и `AsyncLocal<T>`

`ExecutionContext` — контейнер неявных данных (`AsyncLocal<T>`, часть информации безопасности), который **течёт** через `await` и `Task.Run`: продолжение видит значения, которые были при начале ожидания. Обратного потока нет — изменения внутри `async`-метода не видны вызывающему после возврата:

```csharp
var al = new AsyncLocal<string>();
al.Value = "outer";
await Task.Run(async () => { Console.WriteLine(al.Value); al.Value = "inner"; await Task.Delay(1); });   // outer
Console.WriteLine(al.Value);                                                                              // outer

static async Task SetIt(AsyncLocal<string> a) { a.Value = "set in async method"; await Task.Yield(); }
static void SetSync(AsyncLocal<string> a) { a.Value = "set in sync method"; }

await SetIt(al);  Console.WriteLine(al.Value);   // outer
SetSync(al);      Console.WriteLine(al.Value);   // set in sync method
```

Билдер сохраняет и восстанавливает `ExecutionContext` вокруг `async`-метода, поэтому «установить значение в вызванном async-методе» не получится. Практическое следствие: `AsyncLocal` годится для корреляционных идентификаторов вниз по цепочке вызовов, но не для передачи результатов вверх. Подробности `AsyncLocal` — в [18 · Многопоточность](../18-concurrency/expert.ru.md).

### `TaskCompletionSource` и синхронные продолжения

По умолчанию `SetResult` выполняет продолжения подписчиков **синхронно, в вызывающем потоке**. Если подписчик долго работает, он блокирует того, кто вызвал `SetResult`, а при удержании блокировки может дать deadlock. Проверено: продолжение с `Thread.Sleep(300)`:

```csharp
var tcs = new TaskCompletionSource<int>();                                           // default
var w = Task.Run(async () => { await tcs.Task; Thread.Sleep(300); });
await Task.Delay(50);
tcs.SetResult(1);                                                                    // blocked ~300 ms

var tcs2 = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
// the same waiter: SetResult returns in ~0 ms, the continuation runs on the pool
```

```text
default: SetResult blocked ~300 ms (continuation ran inline)
RunContinuationsAsynchronously: SetResult blocked ~0 ms
```

Если `TaskCompletionSource` создаётся в библиотечном коде, почти всегда нужен `RunContinuationsAsynchronously`.

## Справочник

| API | Назначение | С версии |
|---|---|---|
| `Task.Run(Func<T>)` | запустить CPU-работу на пуле | 4.5 |
| `Task.WhenAll` / `WhenAny` | дождаться всех / первой | 4.5 |
| `Task.WhenEach` | асинхронный поток задач по мере завершения | .NET 9 |
| `Task.Delay(ms, ct)` | асинхронная пауза | 4.5 |
| `Task.Yield()` | принудительно приостановиться | 4.5 |
| `Task.FromResult` / `CompletedTask` / `FromCanceled` / `FromException` | готовые задачи | 4.5+ |
| `Task.WaitAsync(timeout, ct)` | прекратить ожидание по таймауту/токену | .NET 6 |
| `ConfigureAwait(bool)` / `ConfigureAwait(ConfigureAwaitOptions)` | управление контекстом / подавлением исключений | 4.5 / .NET 8 |
| `CancellationTokenSource.CancelAsync()`, `CancelAfter` | отмена | .NET 8 / 4.5 |
| `Parallel.ForEachAsync` | параллельный асинхронный цикл | .NET 6 |
| `PeriodicTimer` | асинхронный таймер | .NET 6 |
| `IAsyncEnumerable<T>`, `await foreach` | асинхронные потоки | C# 8 |
| `IAsyncDisposable`, `await using` | асинхронная очистка | C# 8 |
| `ValueTask`, `ValueTask<T>` | задача без выделения при синхронном завершении | .NET Core 2.1 |
| `TaskCompletionSource<T>` | превратить callback в `Task` | 4.0 |

`ConfigureAwaitOptions.SuppressThrowing` позволяет дождаться задачи и **не получить** её исключение:

```csharp
await Fail().ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);   // no exception thrown
```

Задачи, завершившиеся ошибкой, но не дождавшиеся `await`, остаются в `Task.Exception`; `Task.Run(() => throw ...)` после паузы имеет `IsFaulted == true` (проверено).

## Пул потоков и исчерпание

Асинхронный код выигрывает, пока потоки пула свободны. Блокировка (`Task.Wait`, `Thread.Sleep`, синхронный I/O) занимает поток впустую; пул добавляет потоки с задержкой (после достижения минимума — по алгоритму hill-climbing), поэтому под нагрузкой наступает **исчерпание** (starvation): задачи стоят в очереди, а потоки заняты ожиданием других задач. Диагностика: растущая очередь пула (`ThreadPool.PendingWorkItemCount`), задержки при низкой загрузке CPU. Лечение — убрать sync-over-async, а не раздувать `SetMinThreads`.

## Подвохи

**1. `async` без `await` завершается синхронно.**

```csharp
static async Task<int> Quick() => 5;
Console.WriteLine(Quick().IsCompleted);   // True
```

Исключение из такого метода всё равно уйдёт в задачу, а не бросится при вызове.

**2. `Select(async ...)` запускает операции при каждом перечислении.** Запрос ленив; каждый проход по нему создаёт задачи заново:

```csharp
int started = 0;
var query = new[] { 1, 2, 3 }.Select(async i => { Interlocked.Increment(ref started); await Task.Delay(10); return i * i; });
Console.WriteLine($"before enumeration: started = {started}");
var list = query.ToList();
Console.WriteLine($"after ToList: started = {started}");
var results = await Task.WhenAll(query);
Console.WriteLine($"after WhenAll(query): started = {started}; results = {string.Join(",", results)}");
```

```text
before enumeration: started = 0
after ToList: started = 3
after WhenAll(query): started = 6; results = 1,4,9
```

Материализуйте запрос один раз (`ToList()`) и передавайте список в `WhenAll`.

**3. После `await` в консоли вы уже на потоке пула.**

```csharp
Console.WriteLine(Thread.CurrentThread.IsThreadPoolThread);   // False
await Task.Delay(10);
Console.WriteLine(Thread.CurrentThread.IsThreadPoolThread);   // True
```

Это важно для кода, зависимого от потока (`[ThreadStatic]`, COM): после `await` поток другой.

**4. `async void` отправляет исключение в контекст.** Проверено на контексте, который его перехватывает: сообщение «from async void» получено в `Post`. Без контекста исключение необработанно завершит процесс.

**5. Отменённая задача — не `Faulted`.** `Task.Delay(…, ct)` при отмене даёт статус `Canceled`; `catch (Exception)` её поймает, но `task.IsFaulted` — `False`.

**6. Повторное `await` для `ValueTask`.** Если `ValueTask` создан поверх обычного `Task`, второе `await` случайно работает (проверено: обе проверки напечатали `1`), но контракт это запрещает, а для задач на пулируемых источниках (`IValueTaskSource`) результат непредсказуем.

**7. Одинаковые готовые задачи.** `Task.FromResult(true)` возвращает кэшированный экземпляр: `ReferenceEquals(Task.FromResult(true), Task.FromResult(true))` — `True` (проверено). Сравнивать задачи по ссылке нельзя.

**8. `lock` и `await`.** `lock (o) { await ... }` — ошибка компиляции `CS1996`: поток после `await` может быть другим, а `Monitor` привязан к потоку. Нужен `SemaphoreSlim.WaitAsync`.

## Компромиссы и альтернативы

| Задача | Подход |
|---|---|
| Ожидание I/O | `async`/`await` |
| CPU-работа | `Task.Run`, `Parallel`, [потоки](../18-concurrency/overview.ru.md) |
| Поток событий во времени | `IAsyncEnumerable<T>`, `Channel<T>` |
| Колбэк → задача | `TaskCompletionSource` |
| Конвейер с ограничением | `Parallel.ForEachAsync`, `SemaphoreSlim`, `Channel<T>` |
| Реактивные потоки | Rx.NET (внешняя библиотека) |

## Вопросы с собеседований

<details><summary>1. Чем `async`/`await` отличается от создания потока?</summary>

`async` не создаёт потоков. Метод приостанавливается на `await`, а поток возвращается вызывающему. Операции ввода-вывода завершаются средствами ОС, и продолжение запланировано на поток пула. Поток нужен только на время самого кода, а не на время ожидания.

</details>

<details><summary>2. Что делает компилятор с `async`-методом?</summary>

Превращает его в конечный автомат (`IAsyncStateMachine`), поля которого хранят локальные переменные, пережившие `await`. Билдер (`AsyncTaskMethodBuilder`) создаёт возвращаемую задачу и планирует продолжение.

</details>

<details><summary>3. Когда `await` не приостанавливает метод?</summary>

Когда `awaiter.IsCompleted == true`: задача уже завершена (готовый `Task.FromResult`, кэшированный результат). Тогда выполнение идёт синхронно без выделения продолжения.

</details>

<details><summary>4. Чем отличаются `await task` и `task.Result`?</summary>

`await` не блокирует поток и разворачивает исключение; `.Result` блокирует поток и оборачивает исключение в `AggregateException`. Блокировка может привести к deadlock в контексте синхронизации и исчерпанию пула.

</details>

<details><summary>5. Объясните deadlock с `.Result` в UI.</summary>

Метод вызывает `.Result`, блокируя UI-поток. Внутри `await` хочет вернуть продолжение в `SynchronizationContext` UI, но поток занят ожиданием задачи. Каждый ждёт другого. Лечение: `await` по всей цепочке или `ConfigureAwait(false)` в библиотечном коде.

</details>

<details><summary>6. Что делает `ConfigureAwait(false)` и в ASP.NET Core?</summary>

Отказывается от возврата в захваченный контекст. В ASP.NET Core контекста синхронизации нет, поэтому разницы практически нет; в библиотеках его всё равно ставят, чтобы код работал в любом окружении.

</details>

<details><summary>7. Почему `async void` опасен?</summary>

Нет возвращаемой задачи: нельзя дождаться, нельзя поймать исключение вызывающим. Исключение уходит в `SynchronizationContext` или завершает процесс. Допустим для обработчиков событий.

</details>

<details><summary>8. Что произойдёт при `Task.WhenAll`, если упадут две задачи?</summary>

`await` бросит первое исключение, остальные доступны в `Task.Exception.InnerExceptions`. Поэтому после `WhenAll` в `catch` проверяйте саму задачу, если важны все ошибки.

</details>

<details><summary>9. Когда использовать `ValueTask`?</summary>

Когда метод вызывается очень часто и обычно завершается синхронно (кэш, буфер). Правила: `await` один раз, не использовать параллельно, не читать `.Result` до завершения; для сохранения — `AsTask()`.

</details>

<details><summary>10. Как отменить асинхронную операцию?</summary>

Передать `CancellationToken`; операция проверяет его (`ThrowIfCancellationRequested`) или передаёт в API, который поддерживает токен. Отмена кооперативная: код, не проверяющий токен, не прервётся.

</details>

<details><summary>11. Чем `Task.Run` отличается от `Task.Factory.StartNew`?</summary>

`Task.Run` всегда использует пул и распаковывает вложенные задачи (`Task<Task>` → `Task`); `StartNew` использует `TaskScheduler.Current` и возвращает `Task<Task>` для `async`-лямбд. Правильный выбор — `Task.Run`.

</details>

<details><summary>12. Как превратить callback-API в `Task`?</summary>

Через `TaskCompletionSource<T>`: возвращаем `tcs.Task`, в колбэке вызываем `SetResult`/`SetException`. Создавайте его с `RunContinuationsAsynchronously`, иначе продолжения выполнятся синхронно внутри `SetResult`.

</details>

<details><summary>13. Что такое «async all the way»?</summary>

Принцип: если вызывается асинхронный метод, вызывающий тоже асинхронный и делает `await` до самой точки входа. Смешивание синхронного и асинхронного кода порождает deadlock и исчерпание пула.

</details>

<details><summary>14. Почему `lock` нельзя использовать с `await`?</summary>

`Monitor` привязан к потоку: после `await` выполнение может продолжиться на другом потоке и не сможет освободить блокировку. Компилятор запрещает это (`CS1996`). Используйте `SemaphoreSlim.WaitAsync`.

</details>

<details><summary>15. Вопрос с подвохом: изменится ли `AsyncLocal`, установленный внутри `async`-метода, у вызывающего?</summary>

Нет. `ExecutionContext` течёт вниз и вперёд по цепочке `await`, но восстанавливается при возврате из async-метода. Проверено: после `await SetIt(al)` значение осталось `outer`; синхронный метод изменение сохранил.

</details>

<details><summary>16. Вопрос с подвохом: сколько секунд выполнится `await a(); await b();`, если каждая по 1 с, и как ускорить?</summary>

2 секунды: операции идут последовательно. Нужно сначала запустить обе (`var ta = a(); var tb = b();`), затем `await Task.WhenAll(ta, tb)` — ~1 с.

</details>

<details><summary>17. Что такое исчерпание пула потоков и как его распознать?</summary>

Все потоки пула заблокированы (чаще — sync-over-async), новые задачи ждут в очереди, пул добавляет потоки с задержкой. Признаки: рост задержек при низкой загрузке CPU и растущая очередь пула. Лечение — убрать блокирующие вызовы.

</details>

## Связанные материалы

- Назад: [продвинутая теория](advanced.ru.md). Проверка: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [18 · Многопоточность](../18-concurrency/overview.ru.md) — `AsyncLocal`, пул потоков, `SemaphoreSlim`, `Channel<T>`.
- [08 · Исключения](../08-exceptions/overview.ru.md), [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md) — лямбды и исключения в async-коде.
- [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — выделения памяти конечным автоматом, `IAsyncDisposable`.
- [Python](../../../python/overview.ru.md) — `asyncio`: корутины, цикл событий, `Task`.

## Источники

- [Task-based asynchronous pattern (TAP)](https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Async in depth](https://learn.microsoft.com/dotnet/standard/async-in-depth)
- [`ConfigureAwaitOptions`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.configureawaitoptions)
- [`TaskCreationOptions.RunContinuationsAsynchronously`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.taskcreationoptions)
- [`AsyncLocal<T>`](https://learn.microsoft.com/dotnet/api/system.threading.asynclocal-1)
- [`IAsyncStateMachine`](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.iasyncstatemachine)
